using AdmissionsCommittee.Core.Data;
using AdmissionsCommittee.Core.Domain;
using AdmissionsCommittee.Core.Domain.Filters;
using AdmissionsCommittee.Core.Options;
using AdmissionsCommittee.Data.Helpers;
using Dapper;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdmissionsCommittee.Data.Repository
{
    public class SpecialtyRepository : BaseRepository<Speciality>, ISpecialtyRepository
    {
        public SpecialtyRepository(RepositoryConfiguration sqlConfiguration, IQueryBuilder queryBuilder) : base(sqlConfiguration, queryBuilder)
        {
        }

        public override async Task<IEnumerable<Speciality>> GetAllAsync()
        {
            var query = QueryBuilder.MsSqlQueryToString(GetAllQuery());

            var speciality = await Connection.QueryAsync<Speciality, Faculty, Coefficient, Eie, Speciality>(query,
                (speciality, faculty, coef, eie) =>
                {
                    speciality.Faculty = faculty;
                    coef.Eie = eie;
                    speciality.Coefficients.Add(coef);

                    return speciality;
                }, splitOn: $"{nameof(Faculty.FacultyId)}, {nameof(Coefficient.CoefficientValue)},  {nameof(Eie.EieId)}");

            var result = speciality.GroupBy(p => p.SpecialityId).Select(g =>
            {
                var groupedSpeciality = g.First();
                var temp = g.Select(p => p.Coefficients.First()).ToList();
                groupedSpeciality.Coefficients = temp;
                return groupedSpeciality;
            });
            return result;
        }

        public async override Task<IEnumerable<Speciality>> PaginateAsync(PaginationFilter paginationFilter, SortFilter? sortFilter, DynamicFilters? dynamicFilters)
        {
            QueryBuilder.GetAllQuery = GetAllQuery();
            QueryBuilder.TableName = nameof(Speciality);

            var query = QueryBuilder.PaginateFilter(paginationFilter, sortFilter, dynamicFilters);
            var coeff = await Connection.QueryAsync("SELECT * FROM Coefficient");

            var speciality = await Connection.QueryAsync<Speciality, Faculty, Coefficient, Eie, Speciality>(query,
                (speciality,faculty, coef, eie) =>
                {
                    speciality.Faculty = faculty;
                    coef.Eie = eie;
                    speciality.Coefficients.Add(coef);
                   
                    return speciality;
                }, splitOn: $"{nameof(Faculty.FacultyId)}, {nameof(Coefficient.CoefficientValue)},  {nameof(Eie.EieId)}");

            var result = speciality.GroupBy(p => p.SpecialityId).Select(g =>
            {
                var groupedSpeciality = g.First();
                var temp = g.Select(p => p.Coefficients.First()).ToList();
                groupedSpeciality.Coefficients = temp;
                return groupedSpeciality;
            });
            return result;
        }

        private Query GetAllQuery()
        {
            var facultyTableName = nameof(Faculty);
            var coefsTableName = nameof(Coefficient);
            var eieTablename = nameof(Eie);

            var query = new Query(TableName)
                .Join(facultyTableName, $"{facultyTableName}.{nameof(Faculty.FacultyId)}",
                    $"{TableName}.{nameof(Speciality.FacultyId)}")
                .LeftJoin(coefsTableName, $"{coefsTableName}.{nameof(Coefficient.SpecialityId)}",
                    $"{TableName}.{nameof(Speciality.SpecialityId)}")
                .LeftJoin(eieTablename, $"{eieTablename}.{nameof(Eie.EieId)}",
                    $"{coefsTableName}.{nameof(Coefficient.EieId)}");

            return query;
        }
    }
}
