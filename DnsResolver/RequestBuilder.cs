using DNS.Protocol;

namespace DNS_server;

public class RequestBuilder : IRequestBuilder
{
    private readonly Request request = new();

    public IRequestBuilder AddQuestion(Domain domain, RecordType recordType, RecordClass recordClass = RecordClass.IN)
    {
        var question = new Question(domain, recordType, recordClass);
        request.Questions.Add(question);
        return this;
    }

    public IRequestBuilder WithOperationCode(OperationCode opCode)
    {
        request.OperationCode = opCode;
        return this;
    }

    public IRequestBuilder RecursionDesired(bool recursionDesired)
    {
        request.RecursionDesired = recursionDesired;
        return this;
    }

    public byte[] Build() => request.ToArray();
}