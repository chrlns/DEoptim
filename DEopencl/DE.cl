
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

inline uint rotl(const uint x, int k) {
    return (x << k) | (x >> (32 - k));
}

uint xoroshiro64star_next(__global uint s[2]) {
	const uint s0 = s[0];
	uint s1 = s[1];
	const uint result_star = s0 * 0x9E3779BB;

	s1 ^= s0;
	s[0] = rotl(s0, 26) ^ s1 ^ (s1 << 9); // a, b
	s[1] = rotl(s1, 13); // c

	return result_star;
}

uint rng_next_int(__global uint* seed, uint max) {
    return xoroshiro64star_next(seed) % max;
}

float rng_next_float(__global uint* seed) {
    return xoroshiro64star_next(seed) / 4294967295.0f; // 2^32 as float
}

float rng_next_float_range(__global uint* seed, float min, float max) {
    uint r = xoroshiro64star_next(seed);
    float d =  4294967295.0f / (max - min);
    return (r / d) - (max - min) / 2.0f;
}

/**
 *  Initializes the population buffer with random values.
 *
 *  @param population Buffer holding the complete population.
 *  @param each Number of float that represent one individual.
 *  @param seed for the PRNG.
 */
__kernel 
void population_init (__global float* population, uint attr, 
                      __global float* attr_min_limit, __global float* attr_max_limit, 
                      __global uint* seed) 
{
    __private size_t id = get_global_linear_id();
    seed = seed + 2 * id;

    id = id * attr;

    for (uint a = 0; a < attr; a++) {
        float rnd = rng_next_float_range(seed, attr_min_limit[a], attr_max_limit[a]);
        population[id + a] = rnd;
    }
}

__kernel
void population_mutate(uint NP, float F, __global float* pop, __global float* pop_new, 
                       uint attr, __global float* attr_min_limit, __global float* attr_max_limit, 
                       __global uint* seed) 
{
    __private size_t id = get_global_linear_id();
    seed = seed + 2 * id;

    id = id * attr;

    uint r1 = rng_next_int(seed, NP) * attr;
    uint r2 = rng_next_int(seed, NP) * attr;
    uint r3 = rng_next_int(seed, NP) * attr;

    for (uint a = 0; a < attr; a++) {
        // DE/rand/1/bin
        pop_new[id + a] = pop[r1 + a] + F * (pop[r2 + a] - pop[r3 + a]);
    }
}

__kernel
void population_cross(float CR, __global float* pop, __global float* pop_new, 
                      uint attr, __global uint* seed) 
{
    __private size_t id = get_global_linear_id();
    seed = seed + 2 * id;

    id = id * attr;

    for (uint a = 0; a < attr; a++) {
        float r = rng_next_float(seed);
        if (r > CR) {
            // Choose the original attribute
            pop_new[id + a] = pop[id + a];
        } // else use the new mutated one
    }
}

inline float bench_f1(__global float x[3]) {
    float sum = 0.0f;
    for (int n = 0; n < 3; n++) {
        sum = sum + (x[n] * x[n]);
    }
    return sum;
}

float fitness(__global float x[3]) {
    return bench_f1(x);
}

__kernel
void population_select(__global float* pop, __global float* pop_new, uint attr) {
    __private size_t id = get_global_linear_id() * attr;

    float fitness_old = fitness(pop + id);
    float fitness_new = fitness(pop_new + id);
    if (fitness_new >= fitness_old) {
        for (int a = 0; a < attr; a++) {
            pop_new[id + a] = pop[id + a]; // TODO memcpy?
        }
    }
}
