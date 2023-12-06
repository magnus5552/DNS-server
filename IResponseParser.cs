using DNS.Protocol;

namespace DNS_server;

public interface IResponseParser
{
    Response Parse(byte[] response);
}