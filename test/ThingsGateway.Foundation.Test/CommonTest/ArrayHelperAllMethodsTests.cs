using System.Buffers;
using System.Text;
using System.Text.Json.Nodes;

using ThingsGateway.Foundation.Common.Extension;
#pragma warning disable CA1861 // 不要将常量数组作为参数

namespace ThingsGateway.Foundation.Common.Tests
{
    [TestClass]
    public class ArrayHelperAllMethodsTests
    {
        [TestMethod]
        public async Task AllMethods_BasicCoverage()
        {
            // ToArray
            var list = new List<int> { 1, 2, 3 };
            var arr = list.ToArray();
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, arr);

            // ToStr
            byte[] bytes = Encoding.UTF8.GetBytes("abc");
            Assert.AreEqual("abc", ((ReadOnlySpan<byte>)bytes).ToStr());
            Assert.AreEqual("abc", ((Span<byte>)bytes).ToStr());

            // GetBytes (encoding)
            var chars = "hi".AsSpan();
            Span<byte> dest = stackalloc byte[10];
            var used = Encoding.UTF8.GetBytes(chars, dest);
            Assert.IsGreaterThan(0, used);

            // GetString (encoding)
            Assert.AreEqual("abc", Encoding.UTF8.GetString((ReadOnlySpan<byte>)bytes));

            // ToHex variants
            var hex = ((ReadOnlySpan<byte>)bytes).ToHex();
            Assert.IsFalse(string.IsNullOrEmpty(hex));
            var hexMax = ((ReadOnlySpan<byte>)bytes).ToHex(2);
            Assert.IsLessThanOrEqualTo(4, hexMax.Length);
            var hexSep = ((ReadOnlySpan<byte>)bytes).ToHex("-", 0);
            Assert.Contains("-", hexSep);

            // Substring and IndexOf
            ReadOnlySpan<byte> src = new byte[] { 0, 1, 2, 3, 4 };
            var start = new byte[] { 1 };
            var end = new byte[] { 3 };
            var sub = src.Substring(start, end);
            CollectionAssert.AreEqual(new byte[] { 2 }, sub.ToArray());
            var idx = src.IndexOf(start, end);
            Assert.AreEqual(2, idx.count);

            // Stream Write / WriteAsync
            using var ms = new MemoryStream();
            var mem = new ReadOnlyMemory<byte>(new byte[] { 9, 8, 7 });
            await ms.WriteAsync(mem).ConfigureAwait(false);
            Assert.AreEqual(3, ms.Length);
            ms.SetLength(0);
            await ms.WriteAsync(mem).ConfigureAwait(false);
            Assert.AreEqual(3, ms.Length);

            // ToJsonArray
            var ja = new int[] { 1, 2 }.ToJsonArray();
            Assert.IsInstanceOfType(ja, typeof(JsonArray));
            var ja2 = ((ReadOnlySpan<int>)new int[] { 3, 4 }).ToJsonArray();
            Assert.IsInstanceOfType(ja2, typeof(JsonArray));

            // HasValue / IsNullOrEmpty / IsIn
            IReadOnlyCollection<int>? rc = null;
            Assert.IsFalse(rc.HasValue());
            Assert.IsTrue(rc.IsNullOrEmpty());
            rc = new List<int> { 5 };
            Assert.IsTrue(rc.HasValue());
            Assert.IsFalse(rc.IsNullOrEmpty());
            Assert.IsTrue(1.IsIn(1, 2));

            // ArrayExpandToLength (array)
            int[] a = { 1, 2 };
            var aExp = a.ArrayExpandToLength(4);
            Assert.HasCount(4, aExp);
            var aShr = aExp.ArrayExpandToLength(1);
            Assert.HasCount(1, aShr);
            int[]? nullArr = null;
            var fromNull = nullArr.ArrayExpandToLength(3);
            Assert.HasCount(3, fromNull);

            // Memory/ReadOnlyMemory/Span/ReadOnlySpan variants
            Memory<int> memi = new int[] { 1, 2, 3 };
            var memExp = memi.ArrayExpandToLength(5);
            Assert.AreEqual(5, memExp.Length);
            ReadOnlyMemory<int> rom = new int[] { 1, 2, 3 };
            var romExp = rom.ArrayExpandToLength(4);
            Assert.AreEqual(4, romExp.Length);
            ReadOnlySpan<int> ros = new int[] { 1, 2, 3 };
            var rosExp = ros.ArrayExpandToLength(4);
            Assert.AreEqual(4, rosExp.Length);
            Span<int> sp = stackalloc int[3];
            sp[0] = 1; sp[1] = 2; sp[2] = 3;
            var spExp = sp.ArrayExpandToLength(4);
            Assert.AreEqual(4, spExp.Length);

            // ArrayExpandToLengthEven
            int[] odd = { 1, 2, 3 };
            var even = odd.ArrayExpandToLengthEven();
            Assert.HasCount(4, even);
            var evenMem = memi.ArrayExpandToLengthEven();
            Assert.AreEqual(4, evenMem.Length);
            var evenRom = rom.ArrayExpandToLengthEven();
            Assert.AreEqual(4, evenRom.Length);

            // ArrayRemoveDouble / Begin / Last
            int[] full = { 1, 2, 3, 4, 5 };
            var removed = ArrayHelper.ArrayRemoveDouble(full, 1, 1);
            CollectionAssert.AreEqual(new[] { 2, 3, 4 }, removed);
            var readOnlySpanRemoved = ArrayHelper.ArrayRemoveLast((ReadOnlySpan<int>)full, 2);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, readOnlySpanRemoved.ToArray());
            var romRemoved = ArrayHelper.ArrayRemoveBegin((ReadOnlyMemory<int>)full, 1);
            CollectionAssert.AreEqual(new[] { 2, 3, 4, 5 }, romRemoved.ToArray());

            // ArraySplitByLength
            var splits = full.ArraySplitByLength(2);
            Assert.HasCount(3, splits);

            // ChunkBetter IEnumerable
            var chunks = full.ChunkBetter(2).ToList();
            Assert.HasCount(3, chunks);

            // ChunkBetter ReadOnlyMemory
            var chunkMem = ((ReadOnlyMemory<int>)full).ChunkBetter(2).ToList();
            Assert.HasCount(3, chunkMem);

            // ChunkBetter ReadOnlySequence
            var seq = new ReadOnlySequence<int>(full);
            var seqChunks = seq.ChunkBetter(2).ToList();
            Assert.HasCount(3, seqChunks);

            // CopyArray
            var copy = full.CopyArray();
            CollectionAssert.AreEqual(full, copy);

            // CreateTwoArrayFromOneArray
            var two = new int[] { 1, 2, 3, 4 };
            var twod = two.CreateTwoArrayFromOneArray(2, 2);
            Assert.AreEqual(1, twod[0, 0]);
            Assert.AreEqual(4, twod[1, 1]);

            // SelectLast / SelectMiddle
            var last = full.SelectLast(2);
            CollectionAssert.AreEqual(new[] { 4, 5 }, last);
            var middle = full.SelectMiddle(1, 3);
            CollectionAssert.AreEqual(new[] { 2, 3, 4 }, middle);

            // BytesAdd
            byte[] bsrc = { 1, 2, 3 };
            var bres = ((ReadOnlySpan<byte>)bsrc).BytesAdd(1);
            CollectionAssert.AreEqual(new byte[] { 2, 3, 4 }, bres);
            var seqBytes = new ReadOnlySequence<byte>(bsrc);
            var seqRes = seqBytes.BytesAdd(1);
            CollectionAssert.AreEqual(new byte[] { 2, 3, 4 }, seqRes);

            // GetAsciiXOR
            byte[] xsrc = { 0x01, 0x02, 0x03 };
            var xor = ((ReadOnlySpan<byte>)xsrc).GetAsciiXOR();
            Assert.HasCount(2, xor);

            // SplitIntegerToArray
            var splitInt = ArrayHelper.SplitIntegerToArray(10, 3);
            Assert.HasCount(4, splitInt);

            // SpliceArray
            var spliced = ArrayHelper.SpliceArray(new int[] { 1 }, new int[] { 2, 3 });
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, spliced);
            var mems = new Memory<int>[] { new int[] { 4, 5 }, new int[] { 6 } };
            var splicedMem = ArrayHelper.SpliceArray(mems);
            CollectionAssert.AreEqual(new[] { 4, 5, 6 }, splicedMem.ToArray());

            // ByteToBoolByte / BoolToByte
            var tbytes = ((ReadOnlySpan<byte>)new byte[] { 1, 0 }).ByteToBoolByte();
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0x00 }, tbytes);
            var tbools = ((ReadOnlySpan<bool>)new bool[] { true, false }).BoolArrayToByte();
            CollectionAssert.AreEqual(new byte[] { 0x01 }, tbools.Take(1).ToArray().Select(b => (byte)b).ToArray());

            // ByteToBoolArray
            var boolarr = ((ReadOnlySpan<byte>)new byte[] { 0b_0000_0011 }).ByteToBoolArray(4);
            CollectionAssert.AreEqual(new bool[] { true, true, false, false }, boolarr);
            var seqBool = new ReadOnlySequence<byte>(new byte[] { 0b_0000_1111 });
            var seqBoolArr = seqBool.ByteToBoolArray(4);
            CollectionAssert.AreEqual(new bool[] { true, true, true, true }, seqBoolArr);

            // ByteBitsToBytes
            var bb = ((ReadOnlySpan<byte>)new byte[] { 0b_0000_0101 }).ByteBitsToBytes(3, 0, 0xFF);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0x00, 0xFF }, bb);

            // ByteToByteArray / ReadOnlySpan
            var compressed = ((ReadOnlySpan<byte>)new byte[] { 1, 0, 1, 0, 1, 0, 1, 1 }).ByteToByteArray();
            CollectionAssert.AreEqual(new byte[] { 0xD5 }, compressed);
            var compressedSeq = new ReadOnlySequence<byte>(new byte[] { 1, 0, 1, 0, 1, 0, 1 });
            var compressedSeqRes = compressedSeq.ByteToByteArray();
            Assert.IsNotEmpty(compressedSeqRes);

            // BoolArrayToByte
            var packed = ((ReadOnlySpan<bool>)new bool[] { true, true, false, false, true, true, true, true }).BoolArrayToByte();
            Assert.HasCount(1, packed);

            // CombineMemoryBlocks
            var blocks = new List<ReadOnlyMemory<byte>> { new byte[] { 1, 2 }, new byte[] { 3 } };
            var combined = ArrayHelper.CombineMemoryBlocks(blocks);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, combined.ToArray());
            var blocksSpan = new ReadOnlyMemory<byte>[] { new byte[] { 4 }, new byte[] { 5, 6 } };
            var combined2 = ArrayHelper.CombineMemoryBlocks(blocksSpan);
            CollectionAssert.AreEqual(new byte[] { 4, 5, 6 }, combined2.ToArray());

            // ArrayToString
            var sarr = new ReadOnlySpan<string>(new string[] { "a", "b" });
            var joined = sarr.ArrayToString();
            Assert.AreEqual("ab", joined);

            // ToHexString ReadOnlySequence and ReadOnlySpan
            var seq2 = new ReadOnlySequence<byte>(new byte[] { 0x1A, 0x2B, 0x3C });
            var hx = ArrayHelper.ToHexString(seq2, '-', 2);
            Assert.Contains("1A", hx);
            var hx2 = ((ReadOnlySpan<byte>)new byte[] { 0x1A, 0x2B }).ToHexString('-');
            Assert.Contains('-', hx2);

            // GetBoolByIndex and BoolOnByteIndex
            Assert.IsTrue(((ReadOnlySpan<byte>)new byte[] { 0b_0000_0010 }).GetBoolByIndex(1));
            Assert.IsTrue(((byte)0b_0000_0001).BoolOnByteIndex(0));
            try { ((byte)0).BoolOnByteIndex(8); Assert.Fail("Expected exception"); } catch (ArgumentOutOfRangeException) { }

            // BytesReverseByWord (ReadOnlySpan/Span/Memory/ReadOnlyMemory)
            var rev = ((ReadOnlySpan<byte>)new byte[] { 0x01, 0x02, 0x03 }).BytesReverseByWord();
            Assert.IsGreaterThanOrEqualTo(2, rev.Length);
            var memRev = ((Memory<byte>)new byte[] { 0x01, 0x02, 0x03 }).BytesReverseByWord();
            CollectionAssert.AreEqual(memRev.ToArray(), new byte[] { 0x02, 0x01, 0x00, 0x03, });

            // ToHexString overloads for Span/byte[]
            var th = ((Span<byte>)new byte[] { 0x1A, 0x2B }).ToHexString('-');
            Assert.Contains('-', th);
            var thb = ((byte[])new byte[] { 0x1A, 0x2B }).ToHexString('-');
            Assert.Contains('-', thb);
        }
    }
}
