using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WifiInfoWpf;

public partial class MainWindow : Window
{
    private readonly HttpClient _httpClient;
    
    public MainWindow()
    {
        InitializeComponent();
        _httpClient = new HttpClient();
        RefreshWifiInfo();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshWifiInfo();
    }

    private void RefreshWifiInfo()
    {
        try
        {
            var wifiInfo = GetWifiInfo();
            SsidText.Text = wifiInfo.SSID;
            BssidText.Text = wifiInfo.BSSID;
            SignalText.Text = wifiInfo.Signal;
            AuthText.Text = wifiInfo.Auth;
            IpText.Text = GetWirelessIPAddress() ?? "未连接";

            var ethernetInfo = GetEthernetInfo();
            EthernetNameText.Text = ethernetInfo.Name;
            EthernetMacText.Text = ethernetInfo.MacAddress;
            EthernetIpText.Text = ethernetInfo.IPAddress;
            EthernetMaskText.Text = ethernetInfo.SubnetMask;
            EthernetGatewayText.Text = ethernetInfo.Gateway;
        }
        catch (Exception ex)
        {
            SsidText.Text = $"错误: {ex.Message}";
        }
    }

    private (string SSID, string BSSID, string Signal, string Auth) GetWifiInfo()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "wlan show interfaces",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var ssid = "-";
            var bssid = "-";
            var signal = "-";
            var auth = "-";

            foreach (var line in output.Split('\n'))
            {
                var l = line.Trim();
                
                if (l.StartsWith("SSID") && !l.StartsWith("BSSID"))
                {
                    var idx = l.IndexOf(':');
                    if (idx > 0) ssid = l.Substring(idx + 1).Trim();
                }
                else if (l.StartsWith("BSSID"))
                {
                    var idx = l.IndexOf(':');
                    if (idx > 0) bssid = l.Substring(idx + 1).Trim();
                }
                else if (l.Contains("信号") || l.Contains("信号强度") || l.Contains("Signal"))
                {
                    var idx = l.IndexOf(':');
                    if (idx > 0) signal = l.Substring(idx + 1).Trim();
                }
                else if (l.Contains("身份验证") || l.Contains("Authentication") || l.Contains("验证"))
                {
                    var idx = l.IndexOf(':');
                    if (idx > 0) auth = l.Substring(idx + 1).Trim();
                }
            }

            if (ssid == "-" || ssid == "")
            {
                foreach (var line in output.Split('\n'))
                {
                    var l = line.Trim();
                    if (l.Contains("SSID") && !l.Contains("BSSID"))
                    {
                        var parts = l.Split(new[] { ':' }, 2);
                        if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                        {
                            ssid = parts[1].Trim();
                            break;
                        }
                    }
                }
            }

            return (ssid, bssid, signal, auth);
        }
        catch { }
        
        return ("未连接", "-", "-", "-");
    }

    private static string? GetWirelessIPAddress()
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 &&
                ni.OperationalStatus == OperationalStatus.Up)
            {
                foreach (var addr in ni.GetIPProperties().UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        return addr.Address.ToString();
                }
            }
        }
        return null;
    }

    private (string Name, string MacAddress, string IPAddress, string SubnetMask, string Gateway) GetEthernetInfo()
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                ni.OperationalStatus == OperationalStatus.Up)
            {
                var mac = ni.GetPhysicalAddress().ToString();
                var macFormatted = string.Join(":", Enumerable.Range(0, mac.Length / 2).Select(i => mac.Substring(i * 2, 2)));
                
                var props = ni.GetIPProperties();
                var ip = "";
                var mask = "";
                var gateway = "";

                foreach (var addr in props.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ip = addr.Address.ToString();
                        mask = addr.IPv4Mask?.ToString() ?? "";
                    }
                }

                foreach (var gw in props.GatewayAddresses)
                {
                    if (gw.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        gateway = gw.Address.ToString();
                        break;
                    }
                }

                return (ni.Description, macFormatted, ip, mask, gateway);
            }
        }

        return ("未找到有线网卡", "-", "-", "-", "-");
    }

    // ===== 测速功能 - 浏览器打开测速网站 =====
    private void OpenSpeedTest_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://test.ustc.edu.cn/",
                UseShellExecute = true
            });
            SpeedStatusText.Text = "已打开中科大测速网站";
        }
        catch (Exception ex)
        {
            SpeedStatusText.Text = $"打开失败: {ex.Message}";
        }
    }

    private void OpenUrl_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string url)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
                SpeedStatusText.Text = "已打开";
            }
        }
        catch (Exception ex)
        {
            SpeedStatusText.Text = $"打开失败: {ex.Message}";
        }
    }
    // ===== 测速功能结束 =====

    private async void DnsButton_Click(object sender, RoutedEventArgs e)
    {
        var host = DnsTextBox.Text.Trim();
        if (string.IsNullOrEmpty(host))
        {
            DnsResultText.Text = "请输入域名";
            return;
        }

        DnsResultText.Text = "正在查询...";
        DnsButton.IsEnabled = false;

        try
        {
            var addresses = await Dns.GetHostAddressesAsync(host);
            var results = addresses.Select(a => $"{a.AddressFamily}: {a}").ToList();
            
            if (results.Count > 0)
                DnsResultText.Text = $"{host} 的DNS记录:\n\n" + string.Join("\n", results);
            else
                DnsResultText.Text = "未找到DNS记录";
        }
        catch (Exception ex)
        {
            DnsResultText.Text = $"查询失败: {ex.Message}";
        }
        finally
        {
            DnsButton.IsEnabled = true;
        }
    }

    private async void PingButton_Click(object sender, RoutedEventArgs e)
    {
        var host = PingHostTextBox.Text.Trim();
        if (string.IsNullOrEmpty(host))
        {
            PingResultText.Text = "请输入地址";
            return;
        }

        PingResultText.Text = "正在 Ping...";
        PingButton.IsEnabled = false;

        try
        {
            using var ping = new Ping();
            var results = new List<string>();
            
            for (int i = 0; i < 4; i++)
            {
                var reply = await ping.SendPingAsync(host, 1000);
                var status = reply.Status == IPStatus.Success ? "✓" : "✗";
                results.Add($"第 {i+1} 次: {status} 时间={reply.RoundtripTime}ms TTL={reply.Options?.Ttl ?? 0}");
                PingResultText.Text = string.Join("\n", results);
            }

            var successful = results.Where(r => r.Contains("✓")).ToList();
            if (successful.Count > 0)
            {
                var avgTime = successful.Select(r => 
                {
                    var match = System.Text.RegularExpressions.Regex.Match(r, @"时间=(\d+)ms");
                    return match.Success ? int.Parse(match.Groups[1].Value) : 0;
                }).Average();
                
                PingResultText.Text = string.Join("\n", results) + $"\n\n平均: {avgTime:F1}ms";
            }
            else
            {
                PingResultText.Text = string.Join("\n", results) + "\n\n请求超时";
            }
        }
        catch (Exception ex)
        {
            PingResultText.Text = $"错误: {ex.Message}";
        }
        finally
        {
            PingButton.IsEnabled = true;
        }
    }

    private async void ScanButton_Click(object sender, RoutedEventArgs e)
    {
        var host = ScanHostTextBox.Text.Trim();
        var portsText = ScanPortTextBox.Text.Trim();

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(portsText))
        {
            ScanResultText.Text = "请输入目标地址和端口";
            return;
        }

        var ports = new List<int>();
        foreach (var part in portsText.Split(','))
        {
            var p = part.Trim();
            if (p.Contains('-'))
            {
                var range = p.Split('-');
                if (range.Length == 2 && int.TryParse(range[0], out var start) && int.TryParse(range[1], out var end))
                {
                    for (int i = start; i <= end && i <= 65535; i++)
                        ports.Add(i);
                }
            }
            else if (int.TryParse(p, out var port) && port >= 1 && port <= 65535)
            {
                ports.Add(port);
            }
        }

        if (ports.Count == 0)
        {
            ScanResultText.Text = "端口格式错误";
            return;
        }

        if (ports.Count > 100)
        {
            ScanResultText.Text = "端口数量超过100个";
            return;
        }

        ScanResultText.Text = $"正在扫描 {host} 的 {ports.Count} 个端口...\n";
        ScanButton.IsEnabled = false;

        try
        {
            var tasks = ports.Select(async port =>
            {
                if (await CheckPortAsync(host, port)) return port;
                return -1;
            });

            var results = await Task.WhenAll(tasks);
            var openPorts = results.Where(p => p > 0).Select(p => p.ToString()).ToList();

            if (openPorts.Count > 0)
                ScanResultText.Text = $"开放端口: {openPorts.Count}\n\n" + string.Join(", ", openPorts);
            else
                ScanResultText.Text = "未发现开放端口";
        }
        catch (Exception ex)
        {
            ScanResultText.Text = $"扫描错误: {ex.Message}";
        }
        finally
        {
            ScanButton.IsEnabled = true;
        }
    }

    private async Task<bool> CheckPortAsync(string host, int port)
    {
        try
        {
            using var client = new TcpClient();
            var connect = client.ConnectAsync(host, port);
            if (await Task.WhenAny(connect, Task.Delay(1000)) == connect)
                return client.Connected;
            return false;
        }
        catch
        {
            return false;
        }
    }

    private async void DiscoveryButton_Click(object sender, RoutedEventArgs e)
    {
        DiscoveryButton.IsEnabled = false;
        DiscoveryStatusText.Text = "正在扫描...";
        DiscoveryResultText.Text = "";

        try
        {
            var localIP = GetLocalIPAddress();
            if (string.IsNullOrEmpty(localIP))
            {
                DiscoveryResultText.Text = "无法获取本地IP地址";
                return;
            }

            var ipParts = localIP.Split('.');
            if (ipParts.Length != 4)
            {
                DiscoveryResultText.Text = "IP地址格式错误";
                return;
            }

            var baseIP = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}";
            DiscoveryStatusText.Text = $"正在扫描 {baseIP}.1 - {baseIP}.254 ...";

            var tasks = Enumerable.Range(1, 254).Select(async i =>
            {
                var targetIP = $"{baseIP}.{i}";
                if (await CheckDeviceOnlineAsync(targetIP))
                    return targetIP;
                return null!;
            });

            var foundIPs = (await Task.WhenAll(tasks)).Where(ip => !string.IsNullOrEmpty(ip)).ToArray();

            if (foundIPs.Length > 0)
            {
                var results = new List<string>();
                foreach (var ip in foundIPs)
                {
                    var hostname = await GetHostnameAsync(ip);
                    results.Add($"✓ {ip} ({hostname})");
                }
                DiscoveryResultText.Text = $"发现 {foundIPs.Length} 台设备:\n\n" + string.Join("\n", results);
            }
            else
            {
                DiscoveryResultText.Text = "未发现在线设备";
            }

            DiscoveryStatusText.Text = $"扫描完成，发现 {foundIPs.Length} 台设备";
        }
        catch (Exception ex)
        {
            DiscoveryResultText.Text = $"扫描错误: {ex.Message}";
        }
        finally
        {
            DiscoveryButton.IsEnabled = true;
        }
    }

    private static string? GetLocalIPAddress()
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus == OperationalStatus.Up)
            {
                foreach (var addr in ni.GetIPProperties().UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        var ip = addr.Address.ToString();
                        if (ip.StartsWith("192.168.") || ip.StartsWith("10.") || ip.StartsWith("172."))
                            return ip;
                    }
                }
            }
        }
        return null;
    }

    private async Task<bool> CheckDeviceOnlineAsync(string ip)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(ip, 500);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> GetHostnameAsync(string ip)
    {
        try
        {
            var hostEntry = await Dns.GetHostEntryAsync(ip);
            return string.IsNullOrEmpty(hostEntry.HostName) ? "未知" : hostEntry.HostName;
        }
        catch
        {
            return "未知";
        }
    }
}
