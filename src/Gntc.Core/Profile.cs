namespace Gntc.Core;

/// <summary>Project-specific labels, not a claim of consensus lexical meanings.</summary>
public static class Profile
{
    public const string Id = "gntc-connectives-1";
    public static readonly IReadOnlyDictionary<string, string> Operations = new Dictionary<string, string>
    {
        ["associate"] = "Process B together with A.",
        ["develop"] = "Treat B as advancing the discourse from A.",
        ["support"] = "Treat B as strengthening A.",
        ["infer"] = "Treat B as a conclusion drawn from A.",
        ["counter"] = "Treat B as countering an expectation activated by A.",
        ["resume"] = "Treat B as returning to a discourse strand associated with A.",
        ["frame"] = "Use B to delimit the circumstances in which A applies.",
        ["introduce"] = "Process B as introduced content associated with A.",
        ["compare"] = "Interpret B by comparison with A.",
        ["restrict"] = "Use B to qualify the applicability of A."
    };
    public static readonly IReadOnlyDictionary<string, string> Relations = new Dictionary<string, string>
    {
        ["addition"] = "B is added to A.",
        ["contrast"] = "B is contrasted with A; no necessary logical contradiction.",
        ["ground"] = "B supplies justification for A, in the specified domain.",
        ["cause"] = "B causes A (direction is B to A).",
        ["result"] = "B results from A (direction is A to B).",
        ["condition"] = "B states a condition for A.",
        ["purpose"] = "B states the purpose of A.",
        ["temporal"] = "B supplies a temporal relation or frame for A; specify order in a note.",
        ["elaboration"] = "B elaborates A.",
        ["comparison"] = "B is compared with A.",
        ["content"] = "B supplies the content of A (for example, a complement).",
        ["exception"] = "B supplies an exception to A.",
        ["alternative"] = "B is offered as an alternative to A.",
        ["correlation"] = "B is correlated with A in a paired construction.",
        ["concession"] = "B acknowledges a circumstance despite which A holds."
    };
    public static readonly string[] Domains = ["content", "epistemic", "act", "discourse", "syntax"];
    public static readonly string[] UnitKinds = ["word", "phrase", "clause", "complex", "verse", "discourse"];
    public static readonly string[] Statuses = ["candidate", "proposed", "reviewed", "rejected"];
}
