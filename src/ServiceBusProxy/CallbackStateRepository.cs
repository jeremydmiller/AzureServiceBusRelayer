using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServiceBusProxy
{
    public class CallbackStateRepository : ICallbackStateRepository
    {
        private readonly AzureServiceBusSettings _settings;

        public CallbackStateRepository(AzureServiceBusSettings settings)
        {
            _settings = settings;
        }

        public async Task Store(Guid correlationId, object state)
        {
            // TODO -- if this needs to be high performance, there are more
            // resource efficient ways to invoke the serialization
            var json = JsonConvert.SerializeObject(state, new JsonSerializerSettings
            {
                // This embeds type information into the JSON so that Newtonsoft
                // can reconstruct it later even not knowing the .Net type upfront
                TypeNameHandling = TypeNameHandling.All
            });
            
            using (var conn = new SqlConnection(_settings.SqlServerConnectionString))
            {
                await conn.OpenAsync();

                var cmd = conn.CreateCommand();
                cmd.CommandText = "insert into AcknowledgementState (id, state) values (@id, @state)";
                cmd.Parameters.Add("id", SqlDbType.UniqueIdentifier).Value = correlationId;
                cmd.Parameters.Add("state", SqlDbType.VarChar).Value = json;

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<object> Find(Guid correlationId)
        {
            string json = null;
            
            using (var conn = new SqlConnection(_settings.SqlServerConnectionString))
            {
                await conn.OpenAsync();

                var cmd = conn.CreateCommand();
                cmd.CommandText = "select from AcknowledgementState where id = @id";
                cmd.Parameters.Add("id", SqlDbType.UniqueIdentifier).Value = correlationId;

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        json = await reader.GetFieldValueAsync<string>(0);
                    }
                }
            }

            if (json == null) return null;

            // Again, there's ways to optimize the following code if that's necessary
            return JsonConvert.DeserializeObject(json);
        }

        public async Task Delete(Guid correlationId)
        {
            using (var conn = new SqlConnection(_settings.SqlServerConnectionString))
            {
                await conn.OpenAsync();

                var cmd = conn.CreateCommand();
                cmd.CommandText = "delete from AcknowledgementState where id = @id";
                cmd.Parameters.Add("id", SqlDbType.UniqueIdentifier).Value = correlationId;

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}