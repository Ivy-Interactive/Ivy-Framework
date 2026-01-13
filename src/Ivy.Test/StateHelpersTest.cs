using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

namespace Ivy.Test;

public class StateHelpersTest
{
    [Fact]
    public void Test1()
    {
        IState<Guid?> state = new State<Guid?>(Guid.NewGuid());
        var foo = state.As<object>().Value;
    }

    // Guid Tests
    [Fact] public void GuidToObject() => CreateConversionDelegateTest(typeof(Guid), typeof(object));
    [Fact] public void ObjectToGuid() => CreateConversionDelegateTest(typeof(object), typeof(Guid));
    [Fact] public void NullableGuidToObject() => CreateConversionDelegateTest(typeof(Guid?), typeof(object));
    [Fact] public void ObjectToNullableGuid() => CreateConversionDelegateTest(typeof(object), typeof(Guid?));
    [Fact] public void StringToGuid() => CreateConversionDelegateTest(typeof(string), typeof(Guid));
    [Fact] public void StringToNullableGuid() => CreateConversionDelegateTest(typeof(string), typeof(Guid?));

    // DateTime Tests
    [Fact] public void DateTimeToObject() => CreateConversionDelegateTest(typeof(DateTime), typeof(object));
    [Fact] public void ObjectToDateTime() => CreateConversionDelegateTest(typeof(object), typeof(DateTime));
    [Fact] public void NullableDateTimeToObject() => CreateConversionDelegateTest(typeof(DateTime?), typeof(object));
    [Fact] public void ObjectToNullableDateTime() => CreateConversionDelegateTest(typeof(object), typeof(DateTime?));
    [Fact] public void StringToDateTime() => CreateConversionDelegateTest(typeof(string), typeof(DateTime));
    [Fact] public void StringToNullableDateTime() => CreateConversionDelegateTest(typeof(string), typeof(DateTime?));

    // DateTimeOffset Tests
    [Fact] public void DateTimeOffsetToObject() => CreateConversionDelegateTest(typeof(DateTimeOffset), typeof(object));
    [Fact] public void ObjectToDateTimeOffset() => CreateConversionDelegateTest(typeof(object), typeof(DateTimeOffset));
    [Fact] public void NullableDateTimeOffsetToObject() => CreateConversionDelegateTest(typeof(DateTimeOffset?), typeof(object));
    [Fact] public void ObjectToNullableDateTimeOffset() => CreateConversionDelegateTest(typeof(object), typeof(DateTimeOffset?));
    [Fact] public void StringToDateTimeOffset() => CreateConversionDelegateTest(typeof(string), typeof(DateTimeOffset));
    [Fact] public void StringToNullableDateTimeOffset() => CreateConversionDelegateTest(typeof(string), typeof(DateTimeOffset?));

    // DateOnly Tests
    [Fact] public void DateOnlyToObject() => CreateConversionDelegateTest(typeof(DateOnly), typeof(object));
    [Fact] public void ObjectToDateOnly() => CreateConversionDelegateTest(typeof(object), typeof(DateOnly));
    [Fact] public void NullableDateOnlyToObject() => CreateConversionDelegateTest(typeof(DateOnly?), typeof(object));
    [Fact] public void ObjectToNullableDateOnly() => CreateConversionDelegateTest(typeof(object), typeof(DateOnly?));
    [Fact] public void StringToDateOnly() => CreateConversionDelegateTest(typeof(string), typeof(DateOnly));
    [Fact] public void StringToNullableDateOnly() => CreateConversionDelegateTest(typeof(string), typeof(DateOnly?));

    // TimeOnly Tests
    [Fact] public void TimeOnlyToObject() => CreateConversionDelegateTest(typeof(TimeOnly), typeof(object));
    [Fact] public void ObjectToTimeOnly() => CreateConversionDelegateTest(typeof(object), typeof(TimeOnly));
    [Fact] public void NullableTimeOnlyToObject() => CreateConversionDelegateTest(typeof(TimeOnly?), typeof(object));
    [Fact] public void ObjectToNullableTimeOnly() => CreateConversionDelegateTest(typeof(object), typeof(TimeOnly?));
    [Fact] public void StringToTimeOnly() => CreateConversionDelegateTest(typeof(string), typeof(TimeOnly));
    [Fact] public void StringToNullableTimeOnly() => CreateConversionDelegateTest(typeof(string), typeof(TimeOnly?));

    // Byte Array Tests
    [Fact] public void ByteArrayToObject() => CreateConversionDelegateTest(typeof(byte[]), typeof(object));
    [Fact] public void ObjectToByteArray() => CreateConversionDelegateTest(typeof(object), typeof(byte[]));
    [Fact] public void StringToByteArray() => CreateConversionDelegateTest(typeof(string), typeof(byte[]));

    // Enum Tests
    public enum TestEnum { Value1, Value2 }
    [Fact] public void EnumToObject() => CreateConversionDelegateTest(typeof(TestEnum), typeof(object));
    [Fact] public void ObjectToEnum() => CreateConversionDelegateTest(typeof(object), typeof(TestEnum));
    [Fact] public void NullableEnumToObject() => CreateConversionDelegateTest(typeof(TestEnum?), typeof(object));
    [Fact] public void ObjectToNullableEnum() => CreateConversionDelegateTest(typeof(object), typeof(TestEnum?));
    [Fact] public void StringToEnum() => CreateConversionDelegateTest(typeof(string), typeof(TestEnum));
    [Fact] public void StringToNullableEnum() => CreateConversionDelegateTest(typeof(string), typeof(TestEnum?));

    // String Tests
    [Fact] public void StringToObject() => CreateConversionDelegateTest(typeof(string), typeof(object));
    [Fact] public void ObjectToString() => CreateConversionDelegateTest(typeof(object), typeof(string));

    // Primitive Type Tests
    [Fact] public void IntToObject() => CreateConversionDelegateTest(typeof(int), typeof(object));
    [Fact] public void ObjectToInt() => CreateConversionDelegateTest(typeof(object), typeof(int));
    [Fact] public void NullableIntToObject() => CreateConversionDelegateTest(typeof(int?), typeof(object));
    [Fact] public void ObjectToNullableInt() => CreateConversionDelegateTest(typeof(object), typeof(int?));
    [Fact] public void StringToInt() => CreateConversionDelegateTest(typeof(string), typeof(int));
    [Fact] public void StringToNullableInt() => CreateConversionDelegateTest(typeof(string), typeof(int?));

    [Fact] public void DoubleToObject() => CreateConversionDelegateTest(typeof(double), typeof(object));
    [Fact] public void ObjectToDouble() => CreateConversionDelegateTest(typeof(object), typeof(double));
    [Fact] public void NullableDoubleToObject() => CreateConversionDelegateTest(typeof(double?), typeof(object));
    [Fact] public void ObjectToNullableDouble() => CreateConversionDelegateTest(typeof(object), typeof(double?));
    [Fact] public void StringToDouble() => CreateConversionDelegateTest(typeof(string), typeof(double));
    [Fact] public void StringToNullableDouble() => CreateConversionDelegateTest(typeof(string), typeof(double?));

    [Fact] public void BoolToObject() => CreateConversionDelegateTest(typeof(bool), typeof(object));
    [Fact] public void ObjectToBool() => CreateConversionDelegateTest(typeof(object), typeof(bool));
    [Fact] public void NullableBoolToObject() => CreateConversionDelegateTest(typeof(bool?), typeof(object));
    [Fact] public void ObjectToNullableBool() => CreateConversionDelegateTest(typeof(object), typeof(bool?));
    [Fact] public void StringToBool() => CreateConversionDelegateTest(typeof(string), typeof(bool));
    [Fact] public void StringToNullableBool() => CreateConversionDelegateTest(typeof(string), typeof(bool?));

    private void CreateConversionDelegateTest(Type one, Type two)
    {
        _ = ConversionDelegateFactory.CreateConversionDelegate(one, two);
    }

}