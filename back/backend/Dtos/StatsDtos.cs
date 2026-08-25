using System;
using System.Collections.Generic;

namespace backend.Dtos
{
    public record TodayStatsResponse(
        int TotalCalls,
        int ActiveCalls,
        int AnsweredCalls,
        int TransferredCalls,
        int MissedCalls,
        int AvgDurationSeconds,
        int AgentsOnline,
        List<HourlyDataPoint> Hourly
    );

    public record QueueStatsResponse(
        int ActiveCount,
        int AgentsOnline,
        List<QueueCallItem> ActiveCalls,
        List<QueueAgentItem> Agents
    );

    public record QueueCallItem(
        int Id,
        string RoomName,
        string CallerId,
        string Status,
        DateTime StartTime,
        int DurationSeconds
    );

    public record QueueAgentItem(
        int Id,
        string Username,
        string Status
    );

    public record AgentStatsDto(
        Guid AgentId,
        string Name,
        string Status,
        int TotalCalls,
        int AvgDurationSeconds,
        DateTime? LastActiveAt
    );

    public record PeriodStatsResponse(
        DateTime From,
        DateTime To,
        int TotalCalls,
        int CompletedCalls,
        int AvgDurationSeconds,
        List<HourlyDataPoint> Hourly
    );

    public record SummaryStatsResponse(
        int TotalCallsToday,
        int TotalCallsThisWeek,
        int TotalCallsThisMonth,
        decimal TotalUsageHours,
        int ActiveSubscriptions,
        int TotalKnowledgeBases
    );

    public record HourlyDataPoint(
        string Hour,
        int Count
    );

    public record IntentStatsDto(
        string Intent,
        int Count,
        decimal Percentage
    );

    public record HealthCheckResponse(
        string Status,
        string Database,
        string Redis,
        string Livekit,
        string Uptime,
        string Version
    );
}