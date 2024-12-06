using KoboldOSC;
using KoboldOSC.Messages;

namespace KoboldOSC.Tests;

[TestClass]
public sealed class SerializationTests
{
    public const string OSC_TEST_PATH = "/osc/unit_tests/testpath";


    // [TestMethod]
    // public void TestMessageSerialize()
    // {
    //     // Create a new message
    //     KOscMessage msg = new(OSC_TEST_PATH);


    // }


    
    [TestMethod]
    public void TestPooledList()
    {
        PooledQueue<int> testList = new(20);


    }
}
