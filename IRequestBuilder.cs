using DNS.Protocol;

namespace DNS_server;

public interface IRequestBuilder
{
    byte[] Build();
    IRequestBuilder AddQuestion(Domain domain, RecordType recordType, RecordClass recordClass = RecordClass.IN);
    IRequestBuilder WithOperationCode(OperationCode opCode);
    IRequestBuilder RecursionDesired(bool recursionDesired);
}