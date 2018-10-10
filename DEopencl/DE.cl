
/* "This is xoroshiro64* 1.0, our best and fastest 32-bit small-state generator
   for 32-bit floating-point numbers. We suggest to use its upper bits for
   floating-point generation, as it is slightly faster than
   xoroshiro64**. It passes all tests we are aware of except for
   linearity tests, as the lowest six bits have low linear complexity, so
   if low linear complexity is not considered an issue (as it is usually
   the case) it can be used to generate 32-bit outputs, too.

   We suggest to use a sign test to extract a random Boolean value, and
   right shifts to extract subsets of bits.

   The state must be seeded so that it is not everywhere zero."
   Original C version written 2016 by David Blackman and Sebastiano Vigna (vigna@acm.org), Public Domain
   */
static inline uint32 rotl(const uint32 x, int k) {
    return (x << k) | (x >> (32 - k));
}

// TODO Where to put that? ?
static uint32_t s[2];

uint32_t xoroshiro64star_next(void) {
	const uint32_t s0 = s[0];
	uint32_t s1 = s[1];
	const uint32_t result_star = s0 * 0x9E3779BB;

	s1 ^= s0;
	s[0] = rotl(s0, 26) ^ s1 ^ (s1 << 9); // a, b
	s[1] = rotl(s1, 13); // c

	return result_star;
}

__kernel void pop_init (__global float2* buf_samples_1d) {

}

