using System.Data;
using CMS.Application.DTOs;
using CMS.Application.Interfaces.Repositories;
using CMS.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace CMS.Infrastructure.Repositories;

public sealed class ConfigurationRepository : IConfigurationRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ConfigurationRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<LookupConfigurationItemDto>> GetLookupItemsAsync(string configType, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Config_GetLookupItems", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ConfigType", configType);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var items = new List<LookupConfigurationItemDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(MapLookup(reader));
        }

        return items;
    }

    public async Task<LookupConfigurationItemDto> CreateLookupItemAsync(string configType, UpsertLookupConfigurationItemRequestDto request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Config_CreateLookupItem", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ConfigType", configType);
        command.Parameters.AddWithValue("@Name", request.Name);
        command.Parameters.AddWithValue("@Code", request.Code);
        command.Parameters.AddWithValue("@Description", (object?)request.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@DisplayOrder", request.DisplayOrder);
        command.Parameters.AddWithValue("@IsActive", request.IsActive);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) throw new InvalidOperationException("Unable to create configuration item.");

        return MapLookup(reader);
    }

    public async Task<LookupConfigurationItemDto> UpdateLookupItemAsync(Guid configurationItemId, string configType, UpsertLookupConfigurationItemRequestDto request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Config_UpdateLookupItem", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ConfigurationItemId", configurationItemId);
        command.Parameters.AddWithValue("@ConfigType", configType);
        command.Parameters.AddWithValue("@Name", request.Name);
        command.Parameters.AddWithValue("@Code", request.Code);
        command.Parameters.AddWithValue("@Description", (object?)request.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@DisplayOrder", request.DisplayOrder);
        command.Parameters.AddWithValue("@IsActive", request.IsActive);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) throw new InvalidOperationException("Configuration item not found.");

        return MapLookup(reader);
    }

    public async Task DeleteLookupItemAsync(Guid configurationItemId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Config_DeleteLookupItem", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ConfigurationItemId", configurationItemId);

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FraudDetectionRuleDto>> GetFraudRulesAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Config_GetFraudRules", connection) { CommandType = CommandType.StoredProcedure };

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rules = new List<FraudDetectionRuleDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rules.Add(MapFraudRule(reader));
        }

        return rules;
    }

    public async Task<FraudDetectionRuleDto> CreateFraudRuleAsync(UpsertFraudDetectionRuleRequestDto request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Config_CreateFraudRule", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@RuleName", request.RuleName);
        command.Parameters.AddWithValue("@RuleExpression", request.RuleExpression);
        command.Parameters.AddWithValue("@SeverityScore", request.SeverityScore);
        command.Parameters.AddWithValue("@Priority", request.Priority);
        command.Parameters.AddWithValue("@IsActive", request.IsActive);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) throw new InvalidOperationException("Unable to create fraud rule.");

        return MapFraudRule(reader);
    }

    public async Task<FraudDetectionRuleDto> UpdateFraudRuleAsync(Guid fraudRuleId, UpsertFraudDetectionRuleRequestDto request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Config_UpdateFraudRule", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@FraudRuleId", fraudRuleId);
        command.Parameters.AddWithValue("@RuleName", request.RuleName);
        command.Parameters.AddWithValue("@RuleExpression", request.RuleExpression);
        command.Parameters.AddWithValue("@SeverityScore", request.SeverityScore);
        command.Parameters.AddWithValue("@Priority", request.Priority);
        command.Parameters.AddWithValue("@IsActive", request.IsActive);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) throw new InvalidOperationException("Fraud rule not found.");

        return MapFraudRule(reader);
    }

    public async Task DeleteFraudRuleAsync(Guid fraudRuleId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Config_DeleteFraudRule", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@FraudRuleId", fraudRuleId);

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkflowSettingDto>> GetWorkflowSettingsAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Config_GetWorkflowSettings", connection) { CommandType = CommandType.StoredProcedure };

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var settings = new List<WorkflowSettingDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            settings.Add(MapWorkflow(reader));
        }

        return settings;
    }

    public async Task<WorkflowSettingDto> CreateWorkflowSettingAsync(UpsertWorkflowSettingRequestDto request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Config_CreateWorkflowSetting", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@WorkflowKey", request.WorkflowKey);
        command.Parameters.AddWithValue("@StepName", request.StepName);
        command.Parameters.AddWithValue("@StepSequence", request.StepSequence);
        command.Parameters.AddWithValue("@AssignedRole", request.AssignedRole);
        command.Parameters.AddWithValue("@SlaHours", request.SlaHours);
        command.Parameters.AddWithValue("@IsActive", request.IsActive);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) throw new InvalidOperationException("Unable to create workflow setting.");

        return MapWorkflow(reader);
    }

    public async Task<WorkflowSettingDto> UpdateWorkflowSettingAsync(Guid workflowSettingId, UpsertWorkflowSettingRequestDto request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Config_UpdateWorkflowSetting", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@WorkflowSettingId", workflowSettingId);
        command.Parameters.AddWithValue("@WorkflowKey", request.WorkflowKey);
        command.Parameters.AddWithValue("@StepName", request.StepName);
        command.Parameters.AddWithValue("@StepSequence", request.StepSequence);
        command.Parameters.AddWithValue("@AssignedRole", request.AssignedRole);
        command.Parameters.AddWithValue("@SlaHours", request.SlaHours);
        command.Parameters.AddWithValue("@IsActive", request.IsActive);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) throw new InvalidOperationException("Workflow setting not found.");

        return MapWorkflow(reader);
    }

    public async Task DeleteWorkflowSettingAsync(Guid workflowSettingId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = new SqlCommand("sp_Config_DeleteWorkflowSetting", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@WorkflowSettingId", workflowSettingId);

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static LookupConfigurationItemDto MapLookup(SqlDataReader reader)
    {
        return new LookupConfigurationItemDto
        {
            ConfigurationItemId = reader.GetGuid(reader.GetOrdinal("ConfigurationItemId")),
            ConfigType = reader.GetString(reader.GetOrdinal("ConfigType")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Code = reader.GetString(reader.GetOrdinal("Code")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            DisplayOrder = reader.GetInt32(reader.GetOrdinal("DisplayOrder")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
        };
    }

    private static FraudDetectionRuleDto MapFraudRule(SqlDataReader reader)
    {
        return new FraudDetectionRuleDto
        {
            FraudRuleId = reader.GetGuid(reader.GetOrdinal("FraudRuleId")),
            RuleName = reader.GetString(reader.GetOrdinal("RuleName")),
            RuleExpression = reader.GetString(reader.GetOrdinal("RuleExpression")),
            SeverityScore = reader.GetInt32(reader.GetOrdinal("SeverityScore")),
            Priority = reader.GetInt32(reader.GetOrdinal("Priority")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
        };
    }

    private static WorkflowSettingDto MapWorkflow(SqlDataReader reader)
    {
        return new WorkflowSettingDto
        {
            WorkflowSettingId = reader.GetGuid(reader.GetOrdinal("WorkflowSettingId")),
            WorkflowKey = reader.GetString(reader.GetOrdinal("WorkflowKey")),
            StepName = reader.GetString(reader.GetOrdinal("StepName")),
            StepSequence = reader.GetInt32(reader.GetOrdinal("StepSequence")),
            AssignedRole = reader.GetString(reader.GetOrdinal("AssignedRole")),
            SlaHours = reader.GetInt32(reader.GetOrdinal("SlaHours")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
        };
    }
}
