# Tedd.MortonEncoding
Efficient z-order curve Morton encoder / decoder for .Net.

"**Z-order**, **Lebesgue curve**, **Morton space filling curve**,  **Morton order** or **Morton code** map multidimensional data to one dimension while preserving locality of the data points." [Wikipedia article](https://en.wikipedia.org/wiki/Z-order_curve).

Available as NuGet package: https://www.nuget.org/packages/Tedd.MortonEncoding/

# Example

    // Take some numbers that illustrate well
    var x = (UInt32)0b00000000_00000000;
    var y = (UInt32)0b00000000_11111111;

	// Encode
    var result = MortonEncoding.Encode(x, y);
    
    // Test that result is now: 0b10101010_10101010
    Assert.Equal("1010101010101010", Convert.ToString(result,2));
    
    // Decode
    MortonEncoding.Decode(result, out var xBack, out var yBack);

    // Test that we got back the same values as we started with
    Assert.Equal(x, xBack);
    Assert.Equal(y, yBack);

		

# Hardware accelleration
For .Net Core 3 and up CPU-instructions PEXT and PDEP are used to speed up calculations for CPU's that support [BMI2.](https://en.wikipedia.org/wiki/Bit_Manipulation_Instruction_Sets#BMI2_%28Bit_Manipulation_Instruction_Set_2%29) Most recent X86/X64 supports this.

In case of .Net 4, .Net core 1/2 and CPU's that don't support BMI2 a software implementation is automatically chosen  instead. For those interested in the details: This approach uses AND, XOR and SHIFT operations to calculate the number. The calculations are around 13 instructions per dimension, so 26-40 instructions. Each calculation rely on the result of prior calculation so reciprocal throughput is not utilized.

The two other common approaches which are for-loop and lookup table are not implemented. The for-look introduces extra instructions for such a small operation. The lookup-table will perform well in a tight loop benchmark, but relies on lookup table occupying precious L1 cache and may cause extra latency if accessed in non-linear order where CPU prefetch is not able to assist (which is likely to happen).

A lookup-table approach is however used in xUnit test to verify the output from both BMI2 and manual bit operation approach.

# Credits
Thanks to Jeroen Baert's [blog entry](https://www.forceflow.be/2013/10/07/morton-encodingdecoding-through-bit-interleaving-implementations/), as well as Julien Bilalte's comment on using BMI2-instructions. The magic numbers and LUT-table helped a lot. LUT-table is used in xUnit tests to verify both BMI2 and manual calculations.
