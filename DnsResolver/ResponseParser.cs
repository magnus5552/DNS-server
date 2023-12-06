using DNS.Protocol;

namespace DNS_server;

public class ResponseParser : IResponseParser
{
    public Response Parse(byte[] response) => Response.FromArray(response);
}