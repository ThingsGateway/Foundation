using System.Buffers;
using System.Text;
using System.Text.Json.Nodes;

using ThingsGateway.Foundation.Common.Extension;
using Xunit;
#pragma warning disable CA1861 // 不要将常量数组作为参数

namespace ThingsGateway.Foundation.Common.Tests
{
    public class ArrayHelperAllMethodsTests
    {
        [Fact]
        public async Task AllMethods_BasicCoverage()
        {
            // ToArray
            var list = new List<int> { 1, 2, 3 };
            var arr = list.ToArray();
            Assert.Equal(new[] { 1, 2, 3 }, arr);

            // ToStr
            byte[] bytes = Encoding.UTF8.GetBytes("abc");
            Assert.Equal("abc", ((ReadOnlySpan<byte>)bytes).ToStr());
            Assert.Equal("abc", ((Span<byte>)bytes).ToStr());

            // GetBytes (encoding)
            var chars = "hi".AsSpan();
            Span<byte> dest = stackalloc byte[10];
            var used = Encoding.UTF8.GetBytes(chars, dest);
            Assert.True(used > 0);

            // GetString (encoding)
            Assert.Equal("abc", Encoding.UTF8.GetString((ReadOnlySpan<byte>)bytes));

            // ToHex variants
            var hex = ((ReadOnlySpan<byte>)bytes).ToHex();
            Assert.False(string.IsNullOrEmpty(hex));
            var hexMax = ((ReadOnlySpan<byte>)bytes).ToHex(2);
            Assert.True(hexMax.Length <= 4);
            var hexSep = ((ReadOnlySpan<byte>)bytes).ToHex("-", 0);
            Assert.Contains("-", hexSep);

            // Substring and IndexOf
            ReadOnlySpan<byte> src = new byte[] { 0, 1, 2, 3, 4 };
            var start = new byte[] { 1 };
            var end = new byte[] { 3 };
            var sub = src.Substring(start, end);
            Assert.Equal(new byte[] { 2 }, sub.ToArray());
            var idx = src.IndexOf(start, end);
            Assert.Equal(2, idx.count);

            // Stream Write / WriteAsync
            using var ms = new MemoryStream();
            var mem = new ReadOnlyMemory<byte>(new byte[] { 9, 8, 7 });
            await ms.WriteAsync(mem);
            Assert.Equal(3, ms.Length);
            ms.SetLength(0);
            await ms.WriteAsync(mem);
            Assert.Equal(3, ms.Length);

            // ToJsonArray
            var ja = new int[] { 1, 2 }.ToJsonArray();
            Assert.IsType<JsonArray>(ja);
            var ja2 = ((ReadOnlySpan<int>)new int[] { 3, 4 }).ToJsonArray();
            Assert.IsType<JsonArray>(ja2);

            // HasValue / IsNullOrEmpty / IsIn
            IReadOnlyCollection<int>? rc = null;
            Assert.False(rc.HasValue());
            Assert.True(rc.IsNullOrEmpty());
            rc = new List<int> { 5 };
            Assert.True(rc.HasValue());
            Assert.False(rc.IsNullOrEmpty());
            Assert.True(1.IsIn(1, 2));

            // ArrayExpandToLength (array)
            int[] a = { 1, 2 };
            var aExp = a.ArrayExpandToLength(4);
            Assert.Equal(4, aExp.Length);
            var aShr = aExp.ArrayExpandToLength(1);
            Assert.Single(aShr);
            int[]? nullArr = null;
            var fromNull = nullArr.ArrayExpandToLength(3);
            Assert.Equal(3, fromNull.Length);

            // Memory/ReadOnlyMemory/Span/ReadOnlySpan variants
            Memory<int> memi = new int[] { 1, 2, 3 };
            var memExp = memi.ArrayExpandToLength(5);
            Assert.Equal(5, memExp.Length);
            ReadOnlyMemory<int> rom = new int[] { 1, 2, 3 };
            var romExp = rom.ArrayExpandToLength(4);
            Assert.Equal(4, romExp.Length);
            ReadOnlySpan<int> ros = new int[] { 1, 2, 3 };
            var rosExp = ros.ArrayExpandToLength(4);
            Assert.Equal(4, rosExp.Length);
            Span<int> sp = stackalloc int[3];
            sp[0] = 1; sp[1] = 2; sp[2] = 3;
            var spExp = sp.ArrayExpandToLength(4);
            Assert.Equal(4, spExp.Length);

            // ArrayExpandToLengthEven
            int[] odd = { 1, 2, 3 };
            var even = odd.ArrayExpandToLengthEven();
            Assert.Equal(4, even.Length);
            var evenMem = memi.ArrayExpandToLengthEven();
            Assert.Equal(4, evenMem.Length);
            var evenRom = rom.ArrayExpandToLengthEven();
            Assert.Equal(4, evenRom.Length);

            // ArrayRemoveDouble / Begin / Last
            int[] full = { 1, 2, 3, 4, 5 };
            var removed = ArrayHelper.ArrayRemoveDouble(full, 1, 1);
            Assert.Equal(new[] { 2, 3, 4 }, removed);
            var readOnlySpanRemoved = ArrayHelper.ArrayRemoveLast((ReadOnlySpan<int>)full, 2);
            Assert.Equal(new[] { 1, 2, 3 }, readOnlySpanRemoved.ToArray());
            var romRemoved = ArrayHelper.ArrayRemoveBegin((ReadOnlyMemory<int>)full, 1);
            Assert.Equal(new[] { 2, 3, 4, 5 }, romRemoved.ToArray());

            // ArraySplitByLength
            var splits = full.ArraySplitByLength(2);
            Assert.Equal(3, splits.Count);

            // ChunkBetter IEnumerable
            var chunks = full.ChunkBetter(2).ToList();
            Assert.Equal(3, chunks.Count);

            // ChunkBetter ReadOnlyMemory
            var chunkMem = ((ReadOnlyMemory<int>)full).ChunkBetter(2).ToList();
            Assert.Equal(3, chunkMem.Count);

            // ChunkBetter ReadOnlySequence
            var seq = new ReadOnlySequence<int>(full);
            var seqChunks = seq.ChunkBetter(2).ToList();
            Assert.Equal(3, seqChunks.Count);

            // CopyArray
            var copy = full.CopyArray();
            Assert.Equal(full, copy);

            // CreateTwoArrayFromOneArray
            var two = new int[] { 1, 2, 3, 4 };
            var twod = two.CreateTwoArrayFromOneArray(2, 2);
            Assert.Equal(1, twod[0, 0]);
            Assert.Equal(4, twod[1, 1]);

            // SelectLast / SelectMiddle
            var last = full.SelectLast(2);
            Assert.Equal(new[] { 4, 5 }, last);
            var middle = full.SelectMiddle(1, 3);
            Assert.Equal(new[] { 2, 3, 4 }, middle);

            // BytesAdd
            byte[] bsrc = { 1, 2, 3 };
            var bres = ((ReadOnlySpan<byte>)bsrc).BytesAdd(1);
            Assert.Equal(new byte[] { 2, 3, 4 }, bres);
            var seqBytes = new ReadOnlySequence<byte>(bsrc);
            var seqRes = seqBytes.BytesAdd(1);
            Assert.Equal(new byte[] { 2, 3, 4 }, seqRes);

            // GetAsciiXOR
            byte[] xsrc = { 0x01, 0x02, 0x03 };
            var xor = ((ReadOnlySpan<byte>)xsrc).GetAsciiXOR();
            Assert.Equal(2, xor.Length);

            // SplitIntegerToArray
            var splitInt = ArrayHelper.SplitIntegerToArray(10, 3);
            Assert.Equal(4, splitInt.Length);

            // SpliceArray
            var spliced = ArrayHelper.SpliceArray(new int[] { 1 }, new int[] { 2, 3 });
            Assert.Equal(new[] { 1, 2, 3 }, spliced);
            var mems = new Memory<int>[] { new int[] { 4, 5 }, new int[] { 6 } };
            var splicedMem = ArrayHelper.SpliceArray(mems);
            Assert.Equal(new[] { 4, 5, 6 }, splicedMem.ToArray());

            // ByteToBoolByte / BoolToByte
            var tbytes = ((ReadOnlySpan<byte>)new byte[] { 1, 0 }).ByteToBoolByte();
            Assert.Equal(new byte[] { 0xFF, 0x00 }, tbytes);
            var tbools = ((ReadOnlySpan<bool>)new bool[] { true, false }).BoolArrayToByte();
            Assert.Equal(new byte[] { 0x01 }, tbools.Take(1).ToArray().Select(b => (byte)b).ToArray());

            // ByteToBoolArray
            var boolarr = ((ReadOnlySpan<byte>)new byte[] { 0b_0000_0011 }).ByteToBoolArray(4);
            Assert.Equal(new bool[] { true, true, false, false }, boolarr);
            var seqBool = new ReadOnlySequence<byte>(new byte[] { 0b_0000_1111 });
            var seqBoolArr = seqBool.ByteToBoolArray(4);
            Assert.Equal(new bool[] { true, true, true, true }, seqBoolArr);

            // ByteBitsToBytes
            var bb = ((ReadOnlySpan<byte>)new byte[] { 0b_0000_0101 }).ByteBitsToBytes(3, 0, 0xFF);
            Assert.Equal(new byte[] { 0xFF, 0x00, 0xFF }, bb);

            // ByteToByteArray / ReadOnlySpan
            var compressed = ((ReadOnlySpan<byte>)new byte[] { 1, 0, 1, 0, 1, 0, 1, 1 }).ByteToByteArray();
            Assert.Equal(new byte[] { 0xD5 }, compressed);
            var compressedSeq = new ReadOnlySequence<byte>(new byte[] { 1, 0, 1, 0, 1, 0, 1 });
            var compressedSeqRes = compressedSeq.ByteToByteArray();
            Assert.NotEmpty(compressedSeqRes);

            // BoolArrayToByte
            var packed = ((ReadOnlySpan<bool>)new bool[] { true, true, false, false, true, true, true, true }).BoolArrayToByte();
            Assert.Single(packed);

            // CombineMemoryBlocks
            var blocks = new List<ReadOnlyMemory<byte>> { new byte[] { 1, 2 }, new byte[] { 3 } };
            var combined = ArrayHelper.CombineMemoryBlocks(blocks);
            Assert.Equal(new byte[] { 1, 2, 3 }, combined.ToArray());
            var blocksSpan = new ReadOnlyMemory<byte>[] { new byte[] { 4 }, new byte[] { 5, 6 } };
            var combined2 = ArrayHelper.CombineMemoryBlocks(blocksSpan);
            Assert.Equal(new byte[] { 4, 5, 6 }, combined2.ToArray());

            // ArrayToString
            var sarr = new ReadOnlySpan<string>(new string[] { "a", "b" });
            var joined = sarr.ArrayToString();
            Assert.Equal("ab", joined);

            // ToHexString ReadOnlySequence and ReadOnlySpan
            var seq2 = new ReadOnlySequence<byte>(new byte[] { 0x1A, 0x2B, 0x3C });
            var hx = ArrayHelper.ToHexString(seq2, '-', 2);
            Assert.Contains("1A", hx);
            var hx2 = ((ReadOnlySpan<byte>)new byte[] { 0x1A, 0x2B }).ToHexString('-');
            Assert.Contains('-', hx2);

            // GetBoolByIndex and BoolOnByteIndex
            Assert.True(((ReadOnlySpan<byte>)new byte[] { 0b_0000_0010 }).GetBoolByIndex(1));
            Assert.True(((byte)0b_0000_0001).BoolOnByteIndex(0));
            try { ((byte)0).BoolOnByteIndex(8); Assert.Fail("Expected exception"); } catch (ArgumentOutOfRangeException) { }

            // BytesReverseByWord (ReadOnlySpan/Span/Memory/ReadOnlyMemory)
            var rev = ((ReadOnlySpan<byte>)new byte[] { 0x01, 0x02, 0x03 }).BytesReverseByWord();
            Assert.True(rev.Length >= 2);
            var memRev = ((Memory<byte>)new byte[] { 0x01, 0x02, 0x03 }).BytesReverseByWord();
            Assert.Equal(memRev.ToArray(), new byte[] { 0x02, 0x01, 0x00, 0x03, });

            // ToHexString overloads for Span/byte[]
            var th = ((Span<byte>)new byte[] { 0x1A, 0x2B }).ToHexString('-');
            Assert.Contains('-', th);
            var thb = ((byte[])new byte[] { 0x1A, 0x2B }).ToHexString('-');
            Assert.Contains('-', thb);
        }
    }
}
