#region License
// author:         Wu, Gary
// created:        1:47 PM
// description:
#endregion

using Xunit;

namespace CommonUtils.Test.TestData;

[CollectionDefinition("NavitecTest")]
public class NavitecTestCollection : ICollectionFixture<NavitecTestSharedFixture>
{

}

public class NavitecTestSharedFixture
{
    public  readonly string Host     = "192.168.0.10";
    public  readonly int    Port     = 22;
    public  readonly string User     = "root";
    public  readonly string Password = "navitrol";

    public readonly string NavitecHomePath = "/home/navitec/";
    public readonly string NavitecInterfaceFile = "/etc/network/interfaces";
    public readonly string NavitecLicenseFile = "/home/navitec/license.txt";
    public readonly string NavitecLicenseKeyFile = "/home/navitec/licensekey.txt";
    public readonly string NavitecParamsFile = "/home/navitec/params.txt";

    public readonly string NavitecMapFileExtension = ".nte";

    public static IEnumerable<object[]> DownloadFileTestData()
    {
        yield return new object[] { "/etc/network/interfaces", "ToolboxTest/ScpHelperTest/DownloadFile" };
        yield return new object[] { "/home/navitec/license.txt", "ToolboxTest/ScpHelperTest/LicenseFile" };
        // Add more test cases here...
    }
}
