using System.Net;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;

public class DisplayIPAddress : MonoBehaviour
{
    public Text ipAddressText;

    void Start()
    {
        // 获取设备的 IPv4 地址
        string ipAddress = GetIPv4Address();

        // 在 UI 中显示 IPv4 地址
        if (ipAddressText != null)
        {
            ipAddressText.text = "IPv4 Address: " + ipAddress;
        }
    }

    string GetIPv4Address()
    {
        string ipAddress = "N/A";

        try
        {
            // 获取设备的所有网络接口
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var networkInterface in networkInterfaces)
            {
                // 过滤掉虚拟网络接口和非活动的接口
                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                    networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    if (networkInterface.OperationalStatus == OperationalStatus.Up)
                    {
                        // 获取 IPv4 地址
                        foreach (var address in networkInterface.GetIPProperties().UnicastAddresses)
                        {
                            if (address.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                ipAddress = address.Address.ToString();
                                break;
                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error getting IPv4 address: " + e.Message);
        }

        return ipAddress;
    }
}
