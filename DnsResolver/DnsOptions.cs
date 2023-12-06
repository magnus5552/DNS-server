namespace DNS_server;

public class DnsOptions
{
    public const string Dns = "Dns";
    
    public string RootServerAddress { get; set; }
    
    public int RemotePort { get; set; }
    
    public string Address { get; set; }
    
    public int Port { get; set; }
}