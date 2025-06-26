using System.Collections.Concurrent;

namespace web_api;

public class QuestionsStore
{
    private readonly Dictionary<long, Question> Questions = new Dictionary<long,Question>();
    private readonly ConcurrentDictionary<long, HashSet<long>> Votes = new ConcurrentDictionary<long, HashSet<long>>();

    private Lock objectLock = new Lock();
    public void Add(Question question)
    {
        var id=Questions.Count + 1;
        question.Id = id;
        Questions.Add(id, question);
    }

    // Question
    // Id
    // OwnerId
    // Votes <---

    // Vote
    // UserId
    // QuestionId




    public void Vote(Vote vote)
    {
        if (!Questions.ContainsKey(vote.questionId)) return;

        var question = Questions[vote.questionId];
        question.Votes++;
        Questions[vote.questionId] = question;
    }

    public List<Question> GetQuestions()
    {
        return Questions.Values.ToList();
    }
}