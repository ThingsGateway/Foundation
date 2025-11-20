using System.Text;

using ThingsGateway.Foundation.Common.Extension;

namespace ThingsGateway.Foundation.Common.Tests
{
    [TestClass]
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

        [TestMethod]
        public void IsFrom_SameType_ReturnsTrue()
        {
            Assert.IsTrue(typeof(string).IsFrom(typeof(string)));
        }

        [TestMethod]
        public void IsFrom_BaseClass_ReturnsTrue()
        {
            Assert.IsTrue(typeof(DerivedClass).IsFrom(typeof(BaseClass)));
        }

        [TestMethod]
        public void IsFrom_UnrelatedType_ReturnsFalse()
        {
            Assert.IsFalse(typeof(string).IsFrom(typeof(int)));
        }

        [TestMethod]
        public void IsFrom_GenericDefinition_ReturnsTrue()
        {
            Assert.IsTrue(typeof(GenericDerived).IsFrom(typeof(GenericBase<>)));
        }

        [TestMethod]
        public void IsFrom_GenericExactMatch_ReturnsTrue()
        {
            Assert.IsTrue(typeof(GenericDerived).IsFrom(typeof(GenericBase<int>)));
        }

        [TestMethod]
        public void IsFrom_Interface_ReturnsTrue()
        {
            Assert.IsTrue(typeof(MyImpl).IsFrom(typeof(IMyInterface)));
        }

        [TestMethod]
        public void IsFrom_Interface_Generic_ReturnsTrue()
        {
            Assert.IsTrue(typeof(List<int>).IsFrom(typeof(IEnumerable<>)));
        }

        [TestMethod]
        public void IsFrom_NullTypes_ReturnsFalse()
        {
            Assert.IsFalse(((Type?)null).IsFrom(null));
        }

        #endregion

        #region ====== ChangeTypeEx Tests ======

        [TestMethod]
        public void ChangeTypeEx_String_To_Int()
        {
            object? result = "123".ChangeTypeEx(typeof(int));
            Assert.AreEqual(123, result);
        }

        [TestMethod]
        public void ChangeTypeEx_String_To_Enum()
        {
            object? result = "A".ChangeTypeEx(typeof(TestEnum));
            Assert.AreEqual(TestEnum.A, result);
        }

        [TestMethod]
        public void ChangeTypeEx_Int_To_Enum()
        {
            object? result = 2.ChangeTypeEx(typeof(TestEnum));
            Assert.AreEqual(TestEnum.B, result);
        }

        [TestMethod]
        public void ChangeTypeEx_Null_To_NullableInt()
        {
            object? result = ((object?)null).ChangeTypeEx(typeof(int?));
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ChangeTypeEx_EmptyString_To_NullableInt()
        {
            object? result = "".ChangeTypeEx(typeof(int?));
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ChangeTypeEx_String_To_DateTime()
        {
            var str = "2025-11-03";
            object? result = str.ChangeTypeEx(typeof(DateTime));
            Assert.AreEqual(DateTime.Parse(str), result);
        }

        [TestMethod]
        public void ChangeTypeEx_String_To_Decimal_With_Currency()
        {
            object? result = "￥99.99".ChangeTypeEx(typeof(decimal));
            Assert.AreEqual(99.99m, result);
        }

        [TestMethod]
        public void ChangeTypeEx_String_To_Guid()
        {
            var g = Guid.NewGuid();
            object? result = g.ToString().ChangeTypeEx(typeof(Guid));
            Assert.AreEqual(g, result);
        }

        [TestMethod]
        public void ChangeTypeEx_String_To_TimeSpan()
        {
            object? result = "01:00:00".ChangeTypeEx(typeof(TimeSpan));
            Assert.AreEqual(TimeSpan.FromHours(1), result);
        }

        [TestMethod]
        public void ChangeTypeEx_String_To_Type()
        {
            object? result = "System.Int32".ChangeTypeEx(typeof(Type));
            Assert.AreEqual(typeof(int), result);
        }

        [TestMethod]
        public void ChangeTypeEx_DBNull_To_String()
        {
            object? result = DBNull.Value.ChangeTypeEx(typeof(string));
            Assert.IsNull(result);
        }

#if NET8_0_OR_GREATER
        [TestMethod]
        public void ChangeTypeEx_String_To_DateOnly()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            object? result = today.ToString("yyyy-MM-dd").ChangeTypeEx(typeof(DateOnly));
            Assert.AreEqual(today, result);
        }

        [TestMethod]
        public void ChangeTypeEx_String_To_TimeOnly()
        {
            var t = new TimeOnly(10, 20, 30);
            object? result = "10:20:30".ChangeTypeEx(typeof(TimeOnly));
            Assert.AreEqual(t, result);
        }

        [TestMethod]
        public void ChangeTypeEx_ByteArray_To_String()
        {
            var str = "测试UTF8";
            object? result = Encoding.UTF8.GetBytes(str).ChangeTypeEx(typeof(string));
            Assert.AreEqual(str, result);
        }
#endif

        #endregion

        #region ====== CreateInstance Tests ======

        private abstract class AbstractBase { }
        private interface IMyGeneric<T> { }
        private class MyGenericImpl<T> : IMyGeneric<T> { }

        [TestMethod]
        public void CreateInstance_ConcreteType_NoParams()
        {
            var obj = typeof(DerivedClass).CreateInstance();
            Assert.IsInstanceOfType(obj, typeof(DerivedClass));
        }

        [TestMethod]
        public void CreateInstance_ValueType_Default()
        {
            var obj = typeof(int).CreateInstance();
            Assert.AreEqual(0, obj);
        }

        [TestMethod]
        public void CreateInstance_String_Default()
        {
            var obj = typeof(string).CreateInstance();
            Assert.AreEqual(string.Empty, obj);
        }

        [TestMethod]
        public void CreateInstance_GenericList_FromInterface()
        {
            var obj = typeof(IList<int>).CreateInstance();
            Assert.IsInstanceOfType(obj, typeof(List<int>));
        }

        [TestMethod]
        public void CreateInstance_Dictionary_FromInterface()
        {
            var obj = typeof(IDictionary<string, int>).CreateInstance();
            Assert.IsInstanceOfType(obj, typeof(Dictionary<string, int>));
        }

        [TestMethod]
        public void CreateInstance_AbstractType_Throws()
        {
            var ex = Assert.ThrowsExactly<Exception>(() =>
    ReflectHelper.CreateInstance(typeof(AbstractBase)));

            // 外层是包装的 Exception，内部应该是 MemberAccessException
            Assert.IsNotNull(ex.InnerException);
            Assert.IsInstanceOfType(ex.InnerException, typeof(MemberAccessException));

        }

        [TestMethod]
        public void CreateInstance_InterfaceType_Throws()
        {
            var ex = Assert.ThrowsExactly<Exception>(() =>
ReflectHelper.CreateInstance(typeof(IMyInterface)));

            // 外层是包装的 Exception，内部应该是 MemberAccessException
            Assert.IsNotNull(ex.InnerException);
            Assert.IsInstanceOfType(ex.InnerException, typeof(MemberAccessException));

        }

        [TestMethod]
        public void CreateInstance_WithConstructorParameter()
        {
            var obj = typeof(Version).CreateInstance("1.2.3");
            Assert.AreEqual(new Version(1, 2, 3), obj);
        }

        [TestMethod]
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
