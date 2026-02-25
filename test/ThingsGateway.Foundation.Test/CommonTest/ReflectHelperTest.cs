using System.Text;

using ThingsGateway.Foundation.Common.Extension;
using Xunit;

namespace ThingsGateway.Foundation.Common.Tests
{
    public class ReflectHelperTests
    {
        private enum TestEnum { None = 0, A = 1, B = 2 }

        private class BaseClass { }
        private class DerivedClass : BaseClass { }
        private class GenericBase<T> { }
        private class GenericDerived : GenericBase<int> { }

        private interface IMyInterface { }
        private class MyImpl : IMyInterface { }

        #region ====== IsFrom Tests ======

        [Fact]
        public void IsFrom_SameType_ReturnsTrue()
        {
            Assert.True(typeof(string).IsFrom(typeof(string)));
        }

        [Fact]
        public void IsFrom_BaseClass_ReturnsTrue()
        {
            Assert.True(typeof(DerivedClass).IsFrom(typeof(BaseClass)));
        }

        [Fact]
        public void IsFrom_UnrelatedType_ReturnsFalse()
        {
            Assert.False(typeof(string).IsFrom(typeof(int)));
        }

        [Fact]
        public void IsFrom_GenericDefinition_ReturnsTrue()
        {
            Assert.True(typeof(GenericDerived).IsFrom(typeof(GenericBase<>)));
        }

        [Fact]
        public void IsFrom_GenericExactMatch_ReturnsTrue()
        {
            Assert.True(typeof(GenericDerived).IsFrom(typeof(GenericBase<int>)));
        }

        [Fact]
        public void IsFrom_Interface_ReturnsTrue()
        {
            Assert.True(typeof(MyImpl).IsFrom(typeof(IMyInterface)));
        }

        [Fact]
        public void IsFrom_Interface_Generic_ReturnsTrue()
        {
            Assert.True(typeof(List<int>).IsFrom(typeof(IEnumerable<>)));
        }

        [Fact]
        public void IsFrom_NullTypes_ReturnsFalse()
        {
            Assert.False(((Type?)null).IsFrom(null));
        }

        #endregion

        #region ====== ChangeTypeEx Tests ======

        [Fact]
        public void ChangeTypeEx_String_To_Int()
        {
            object? result = "123".ChangeTypeEx(typeof(int));
            Assert.Equal(123, result);
        }

        [Fact]
        public void ChangeTypeEx_String_To_Enum()
        {
            object? result = "A".ChangeTypeEx(typeof(TestEnum));
            Assert.Equal(TestEnum.A, result);
        }

        [Fact]
        public void ChangeTypeEx_Int_To_Enum()
        {
            object? result = 2.ChangeTypeEx(typeof(TestEnum));
            Assert.Equal(TestEnum.B, result);
        }

        [Fact]
        public void ChangeTypeEx_Null_To_NullableInt()
        {
            object? result = ((object?)null).ChangeTypeEx(typeof(int?));
            Assert.Null(result);
        }

        [Fact]
        public void ChangeTypeEx_EmptyString_To_NullableInt()
        {
            object? result = "".ChangeTypeEx(typeof(int?));
            Assert.Null(result);
        }

        [Fact]
        public void ChangeTypeEx_String_To_DateTime()
        {
            var str = "2025-11-03";
            object? result = str.ChangeTypeEx(typeof(DateTime));
            Assert.Equal(DateTime.Parse(str), result);
        }

        [Fact]
        public void ChangeTypeEx_String_To_Decimal_With_Currency()
        {
            object? result = "￥99.99".ChangeTypeEx(typeof(decimal));
            Assert.Equal(99.99m, result);
        }

        [Fact]
        public void ChangeTypeEx_String_To_Guid()
        {
            var g = Guid.NewGuid();
            object? result = g.ToString().ChangeTypeEx(typeof(Guid));
            Assert.Equal(g, result);
        }

        [Fact]
        public void ChangeTypeEx_String_To_TimeSpan()
        {
            object? result = "01:00:00".ChangeTypeEx(typeof(TimeSpan));
            Assert.Equal(TimeSpan.FromHours(1), result);
        }

        [Fact]
        public void ChangeTypeEx_String_To_Type()
        {
            object? result = "System.Int32".ChangeTypeEx(typeof(Type));
            Assert.Equal(typeof(int), result);
        }

        [Fact]
        public void ChangeTypeEx_DBNull_To_String()
        {
            object? result = DBNull.Value.ChangeTypeEx(typeof(string));
            Assert.Null(result);
        }

#if NET8_0_OR_GREATER
        [Fact]
        public void ChangeTypeEx_String_To_DateOnly()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            object? result = today.ToString("yyyy-MM-dd").ChangeTypeEx(typeof(DateOnly));
            Assert.Equal(today, result);
        }

        [Fact]
        public void ChangeTypeEx_String_To_TimeOnly()
        {
            var t = new TimeOnly(10, 20, 30);
            object? result = "10:20:30".ChangeTypeEx(typeof(TimeOnly));
            Assert.Equal(t, result);
        }

        [Fact]
        public void ChangeTypeEx_ByteArray_To_String()
        {
            var str = "测试UTF8";
            object? result = Encoding.UTF8.GetBytes(str).ChangeTypeEx(typeof(string));
            Assert.Equal(str, result);
        }
#endif

        #endregion

        #region ====== CreateInstance Tests ======

        private abstract class AbstractBase { }
        private interface IMyGeneric<T> { }
        private class MyGenericImpl<T> : IMyGeneric<T> { }

        [Fact]
        public void CreateInstance_ConcreteType_NoParams()
        {
            var obj = typeof(DerivedClass).CreateInstance();
            Assert.IsType<DerivedClass>(obj);
        }

        [Fact]
        public void CreateInstance_ValueType_Default()
        {
            var obj = typeof(int).CreateInstance();
            Assert.Equal(0, obj);
        }

        [Fact]
        public void CreateInstance_String_Default()
        {
            var obj = typeof(string).CreateInstance();
            Assert.Equal(string.Empty, obj);
        }

        [Fact]
        public void CreateInstance_GenericList_FromInterface()
        {
            var obj = typeof(IList<int>).CreateInstance();
            Assert.IsType<List<int>>(obj);
        }

        [Fact]
        public void CreateInstance_Dictionary_FromInterface()
        {
            var obj = typeof(IDictionary<string, int>).CreateInstance();
            Assert.IsType<Dictionary<string, int>>(obj);
        }

        [Fact]
        public void CreateInstance_AbstractType_Throws()
        {
            var ex = Assert.Throws<Exception>(() =>
    ReflectHelper.CreateInstance(typeof(AbstractBase)));

            // 外层是包装的 Exception，内部应该是 MissingMethodException
            Assert.NotNull(ex.InnerException);
            Assert.IsType<MissingMethodException>(ex.InnerException);

        }

        [Fact]
        public void CreateInstance_InterfaceType_Throws()
        {
            var ex = Assert.Throws<Exception>(() =>
ReflectHelper.CreateInstance(typeof(IMyInterface)));

            // 外层是包装的 Exception，内部应该是 MissingMethodException
            Assert.NotNull(ex.InnerException);
            Assert.IsType<MissingMethodException>(ex.InnerException);

        }

        [Fact]
        public void CreateInstance_WithConstructorParameter()
        {
            var obj = typeof(Version).CreateInstance("1.2.3");
            Assert.Equal(new Version(1, 2, 3), obj);
        }

        [Fact]
        public void CreateInstance_Exception_Message_Is_Readable()
        {
            try
            {
                _ = typeof(Version).CreateInstance(123, "invalid");
                Assert.Fail("Should throw");
            }
            catch (Exception ex)
            {
                Assert.Contains("Fail to create instance of", ex.Message);
            }
        }

        #endregion
    }
}
