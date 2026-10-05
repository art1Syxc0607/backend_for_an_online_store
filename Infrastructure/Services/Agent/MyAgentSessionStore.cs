//using Domain.Entities.Agent;
//using Domain.Exceptions;
//using Infrastructure.Data;
//using Microsoft.Agents.AI;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace Infrastructure.Services.Agent;

//public sealed class MyAgentSessionStore : AgentSessionStore
//{
//    //private readonly AppDbContext _dbContext;
//    private readonly IServiceScopeFactory _scopeFactory;
//    private readonly JsonSerializerOptions _jsonOptions = JsonSerializerOptions.Web;

//    public MyAgentSessionStore(IServiceScopeFactory serviceScopeFactory)
//    {
//        _scopeFactory = serviceScopeFactory;
//    }

//    public override async ValueTask SaveSessionAsync(
//        AIAgent agent,
//        string sessionStoreId,
//        AgentSession session,
//        CancellationToken cancellationToken = default)
//    {
//        // Persist the session using your storage system.
//        //throw new NotImplementedException();

//        using var scope = _scopeFactory.CreateScope();
//        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

//        var serializedSession = await agent.SerializeSessionAsync(session, jsonSerializerOptions
//            : _jsonOptions, cancellationToken:  cancellationToken);
//        string jsonString = serializedSession.GetRawText();

//        var sessionEntity = await dbContext.AgentConversations.FirstOrDefaultAsync(agSes => agSes.AgentConversationId
//        == sessionStoreId, cancellationToken);

//        if (sessionEntity == null)
//        {
//            await dbContext.AgentConversations.AddAsync(new AgentConversationEntity());
//        }
//        else
//        {
//            sessionEntity.JsonString = jsonString;  
//        }

//        await dbContext.SaveChangesAsync(cancellationToken);
//    }

//    public override async ValueTask<AgentSession> GetSessionAsync(
//        AIAgent agent,
//        string sessionStoreId,
//        CancellationToken cancellationToken = default)
//    {
//        // Restore an independent session, or create one when no state exists.
//        //throw new NotImplementedException();

//        using var scope = _scopeFactory.CreateScope();
//        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

//        var sessionEntity = await dbContext.AgentConversations.FirstOrDefaultAsync(agSes => agSes.AgentConversationId
//        == sessionStoreId, cancellationToken);

//        if (sessionEntity == null)
//        {
//            throw new DomainException($"Not found session with Id: {sessionStoreId}");
//        }

//        try
//        {
//            var sessionJsonElement = JsonSerializer.Deserialize<JsonElement>(sessionEntity.JsonString, _jsonOptions);
//            var agentSession = await agent.DeserializeSessionAsync(sessionJsonElement, cancellationToken: 
//                cancellationToken, jsonSerializerOptions: _jsonOptions);

//            return agentSession;
//        }
//        catch (Exception ex)
//        {
//            throw;
//        }

        
//    }

//    public async ValueTask DeleteSessionAsync(
//        AIAgent agent,
//        string sessionStoreId,
//        CancellationToken cancellationToken = default)
//    {
//        // Delete the stored session if it exists.
//        //throw new NotImplementedException();

//        using var scope = _scopeFactory.CreateScope();
//        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

//        var sessionEntity = await dbContext.AgentConversations.FirstOrDefaultAsync(agSes => agSes.AgentConversationId
//        == sessionStoreId, cancellationToken);

//        if (sessionEntity == null)
//        {
//            throw new DomainException($"Not found session with Id: {sessionStoreId}");
//        }

//        dbContext.AgentConversations.Remove(sessionEntity);
//    }
//}
