using NUnit.Framework;
using System.Text.Json;

namespace LT.DigitalOffice.TimeManagementServiceUnitTests.UnitTestLibrary
{
    public static class SerializerAssert
    {
        public static void AreEqual(object expected, object result)
        {
            var expectedJson = JsonSerializer.Serialize(expected);
            var resultJson = JsonSerializer.Serialize(result);

            Assert.AreEqual(expectedJson, resultJson);
        }
    }
}
