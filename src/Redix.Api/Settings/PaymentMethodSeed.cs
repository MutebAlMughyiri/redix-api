using Redix.Api.Structure;

namespace Redix.Api.Settings;

/// <summary>The payment methods of FR-40, held by the default template, from which every approved project takes its own. After the first migration they are data.</summary>
public static class PaymentMethodSeed
{
    private static readonly string[] Names = ["شبكة", "تحويل", "كاش"];

    /// <summary>The template's payment methods, in their order.</summary>
    public static IEnumerable<PaymentMethod> PaymentMethods => Names.Select((name, index) => new PaymentMethod { Id = StableIds.Of($"paymentMethod:{name}"), ProjectId = StructureSeed.TemplateId, Name = name, SortOrder = index });
}
