using AdmissionsCommittee.Core.Data;
using AdmissionsCommittee.Core.Domain;
using AdmissionsCommittee.Core.Options;
using Dapper;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdmissionsCommittee.Data.Repository
{
    public class ApplicantRepository : BaseRepository<Applicant>, IApplicantRepository
    {
        public ApplicantRepository(RepositoryConfiguration sqlConfiguration, IQueryBuilder queryBuilder) : base(sqlConfiguration, queryBuilder)
        {
        }

        public override async Task<Applicant> GetByIdAsync(int id)
        {
            var personTableName = nameof(Person);
            var sql = new Query(TableName).Where(nameof(Applicant.ApplicantId), "=", id)
                .Join(personTableName, $"{personTableName}.{nameof(Person.PersonId)}", $"{TableName}.{nameof(Applicant.ApplicantId)}");
            var query = QueryBuilder.MsSqlQueryToString(sql);

            var applicant = (await Connection.QueryAsync<Applicant, Person, Applicant>(query, (applicant, person) =>
            {
                applicant.Person = person;
                return applicant;
            }, splitOn: nameof(Person.PersonId))).First();

            return applicant;
        }
    }
}
