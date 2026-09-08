using System.Text;

namespace StormWatch.Chat;

public static class Grounding
{
    public static string BuildPrompt(
        string question,
        IReadOnlyList<KnowledgePassage> passages)
    {
        // TODO 4: Delimit retrieved passages and require evidence-linked citations.
        throw new NotImplementedException();
    }

    public static string AppendSources(
        string answer,
        IReadOnlyList<KnowledgePassage> passages)
    {
        // TODO 4: Add a deterministic, de-duplicated Sources section.
        throw new NotImplementedException();
    }
}