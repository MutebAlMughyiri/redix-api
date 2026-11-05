using System.Text.Json;

namespace Redix.Api;

/// <summary>The answer to a refused request: problem details carrying a <c>code</c>, which the app shows as a word. The API sends codes, never words.</summary>
public static class Refusal
{
    /// <summary>A refusal with <paramref name="status"/>, whose <c>code</c> is <paramref name="reason"/> in camelCase, such as <c>projectClosed</c>.</summary>
    public static IResult Of(int status, string reason) => Results.Problem(
        statusCode: status,
        extensions: new Dictionary<string, object?> { ["code"] = JsonNamingPolicy.CamelCase.ConvertName(reason) });

    /// <summary>A refusal that also says what it was refused over: <paramref name="detail"/>, under the name <paramref name="detailName"/>.</summary>
    public static IResult Of(int status, string reason, string detailName, object detail) => Results.Problem(
        statusCode: status,
        extensions: new Dictionary<string, object?> { ["code"] = JsonNamingPolicy.CamelCase.ConvertName(reason), [detailName] = detail });
}
