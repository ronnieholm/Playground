# TrustpilotAesChallenge

This folder holds multiple implementations of an AES decryptor for [cracking an
AES encrypted
message](http://bugfree.dk/blog/2014/07/17/trustpilot-challenge-crack-aes-encrypted-message).

## Motivation

- Learn how to setup a VSCode environment on Linux for Go, C and .NET Core
  development.
- Learn how to profile a Go program.

## Summary of performance

|Platform                        |Go    |C    |.NET Core 3.1/C#|
|--------------------------------|------|-----|----------------|
|Ubuntu 18.04 (physical machine) |0m29s |0m07s|       0m38s     |

CPU info:

    rh@xps:~/git/Playground$ cat /proc/cpuinfo 
    processor       : 0
    vendor_id       : GenuineIntel
    cpu family      : 6
    model           : 142
    model name      : Intel(R) Core(TM) i7-7560U CPU @ 2.40GHz
    stepping        : 9
    cpu MHz         : 2399.998
    cache size      : 4096 KB
    physical id     : 0
    siblings        : 4
    core id         : 0
    cpu cores       : 4
    apicid          : 0
    initial apicid  : 0
    fpu             : yes
    fpu_exception   : yes
    cpuid level     : 22
    wp              : yes
    flags           : fpu vme de pse tsc msr pae mce cx8 apic sep mtrr pge mca cmov pat pse36 clflush mmx fxsr sse sse2 ht syscall nx rdtscp lm constant_tsc rep_good nopl xtopology nonstop_tsc pni pclmulqdq ssse3 cx16 sse4_1 sse4_2 movbe popcnt aes xsave avx rdrand hypervisor lahf_lm abm 3dnowprefetch avx2 rdseed clflushopt
    bugs            :
    bogomips        : 4799.99
    clflush size    : 64
    cache_alignment : 64
    address sizes   : 39 bits physical, 48 bits virtual
    power management:

Observe the presense of CPU flags pclmulqdq (Perform a Carry-Less Multiplication
of Quadword instruction) and aes (Advanced Encryption Standard) which the Go
runtime checks for.

## Go

Linux: runtime and memory use measured by the time command:

    rh@xps:~/git/Playground/TrustpilotAesChallenge/Go$ /usr/bin/time --verbose go run program.go
    5 11 14 11 1 7
    Congra�u�a�ions� You found the decryption key. Are you the One?! ;-)
    You're curious and you're up for a challenge! So we like you already.
    If you'd like to work with us at Trustpilot then email us the code you wrote to find the decryption key together with your resume.
    The email is: thereisnospoon@trustpilot.com
    We'll get in touch with you shortly.

    Best regards
    The developers at Trustpilot

        Command being timed: "go run program.go"
        User time (seconds): 31.45
        System time (seconds): 0.58
        Percent of CPU this job got: 109%
        Elapsed (wall clock) time (h:mm:ss or m:ss): 0:29.25
        Average shared text size (kbytes): 0
        Average unshared data size (kbytes): 0
        Average stack size (kbytes): 0
        Average total size (kbytes): 0
        Maximum resident set size (kbytes): 138840
        Average resident set size (kbytes): 0
        Major (requiring I/O) page faults: 1
        Minor (reclaiming a frame) page faults: 39180
        Voluntary context switches: 84340
        Involuntary context switches: 13076
        Swaps: 0
        File system inputs: 0
        File system outputs: 27848
        Socket messages sent: 0
        Socket messages received: 0
        Signals delivered: 0
        Page size (bytes): 4096
        Exit status: 0

Profiling while the main program is running:

    rh@xps:~/git/Playground/TrustpilotAesChallenge/Golang/src$ go tool pprof -seconds 10 http://localhost:8080/debug/pprof/profile
    Fetching profile over HTTP from http://localhost:8080/debug/pprof/profile?seconds=10
    Please wait... (10s)
    Saved profile in /home/rh/pprof/pprof.program.samples.cpu.001.pb.gz
    File: program
    Type: cpu
    Time: Dec 26, 2017 at 2:01am (CET)
    Duration: 10.19s, Total samples = 8.70s (85.39%)
    Entering interactive mode (type "help" for commands, "o" for options)
    (pprof) top
    Showing nodes accounting for 7460ms, 85.75% of 8700ms total
    Dropped 70 nodes (cum <= 43.50ms)
    Showing top 10 nodes out of 58
        flat  flat%   sum%        cum   cum%
        4980ms 57.24% 57.24%    4980ms 57.24%  crypto/aes.hasGCMAsm /home/rh/Downloads/go/src/crypto/aes/gcm_amd64.s
        760ms  8.74% 65.98%      760ms  8.74%  crypto/aes.decryptBlockAsm /home/rh/Downloads/go/src/crypto/aes/asm_amd64.s
        400ms  4.60% 70.57%     1670ms 19.20%  crypto/cipher.(*cbcDecrypter).CryptBlocks /home/rh/Downloads/go/src/crypto/cipher/cbc.go
        210ms  2.41% 72.99%      210ms  2.41%  crypto/cipher.fastXORBytes /home/rh/Downloads/go/src/crypto/cipher/xor.go
        200ms  2.30% 75.29%      410ms  4.71%  crypto/cipher.xorBytes /home/rh/Downloads/go/src/crypto/cipher/xor.go
        200ms  2.30% 77.59%      200ms  2.30%  runtime.indexbytebody /home/rh/Downloads/go/src/runtime/asm_amd64.s
        200ms  2.30% 79.89%      220ms  2.53%  runtime.scanblock /home/rh/Downloads/go/src/runtime/mgcmark.go
        200ms  2.30% 82.18%      250ms  2.87%  runtime.scanobject /home/rh/Downloads/go/src/runtime/mgcmark.go
        170ms  1.95% 84.14%      170ms  1.95%  runtime.futex /home/rh/Downloads/go/src/runtime/sys_linux_amd64.s
        140ms  1.61% 85.75%      140ms  1.61%  runtime.nextFreeFast /home/rh/Downloads/go/src/runtime/malloc.go
    (pprof) web
    (pprof) list decipher
    Total: 8.70s
    ROUTINE ======================== main.decipher in /home/rh/git/Playground/TrustpilotAesChallenge/Golang/src/program.go
        20ms      8.14s (flat, cum) 93.56% of Total
            .          .     31:		for b = 0; b <= 16; b++ {
            .          .     32:			for c = 0; c <= 16; c++ {
            .          .     33:				for d = 0; d <= 16; d++ {
            .          .     34:					for e = 0; e <= 16; e++ {
            .          .     35:						for f = 0; f <= 16; f++ {
         10ms       10ms     36:							k[0], k[1], k[2], k[3], k[4], k[5] = a, b, c, d, e, f
            .      5.95s     37:							block, err := aes.NewCipher(k)
            .          .     38:							if err != nil {
            .          .     39:								panic(err)
            .          .     40:							}
            .          .     41:
            .      280ms     42:							decryptor := cipher.NewCBCDecrypter(block, iv)
            .      1.67s     43:							decryptor.CryptBlocks(clearText, cipherText)
         10ms      230ms     44:							if bytes.Contains(clearText, token) {
            .          .     45:								fmt.Printf("%v\n%s", k, clearText)
            .          .     46:							}
            .          .     47:						}
            .          .     48:					}
            .          .     49:				}
    (pprof) list NewCipher
    Total: 8.70s
    ROUTINE ======================== crypto/aes.NewCipher in /home/rh/Downloads/go/src/crypto/aes/cipher.go
            0      5.95s (flat, cum) 68.39% of Total
            .          .     34:	default:
            .          .     35:		return nil, KeySizeError(k)
            .          .     36:	case 16, 24, 32:
            .          .     37:		break
            .          .     38:	}
            .      5.95s     39:	return newCipher(key)
            .          .     40:}
            .          .     41:
            .          .     42:// newCipherGeneric creates and returns a new cipher.Block
            .          .     43:// implemented in pure Go.
            .          .     44:func newCipherGeneric(key []byte) (cipher.Block, error) {
    (pprof) list newCipher	
    Total: 8.70s
    ROUTINE ======================== crypto/aes.newCipher in /home/rh/Downloads/go/src/crypto/aes/cipher_amd64.go
        60ms      5.95s (flat, cum) 68.39% of Total
            .          .     18:	aesCipher
            .          .     19:}
            .          .     20:
            .          .     21:var useAsm = cipherhw.AESGCMSupport()
            .          .     22:
        1 0ms       10ms     23:func newCipher(key []byte) (cipher.Block, error) {
            .          .     24:	if !useAsm {
            .          .     25:		return newCipherGeneric(key)
            .          .     26:	}
         10ms       10ms     27:	n := len(key) + 28
         10ms      700ms     28:	c := aesCipherAsm{aesCipher{make([]uint32, n), make([]uint32, n)}}
            .          .     29:	rounds := 10
            .          .     30:	switch len(key) {
            .          .     31:	case 128 / 8:
            .          .     32:		rounds = 10
            .          .     33:	case 192 / 8:
            .          .     34:		rounds = 12
            .          .     35:	case 256 / 8:
            .          .     36:		rounds = 14
            .          .     37:	}
         10ms      130ms     38:	expandKeyAsm(rounds, &key[0], &c.enc[0], &c.dec[0])
         10ms      4.99s     39:	if hasGCMAsm() {
         10ms      110ms     40:		return &aesCipherGCM{c}, nil
            .          .     41:	}
            .          .     42:
            .          .     43:	return &c, nil
            .          .     44:}
            .          .     45:
    (pprof) list hasGCMAsm
    Total: 8.70s
    ROUTINE ======================== crypto/aes.hasGCMAsm in /home/rh/Downloads/go/src/crypto/aes/gcm_amd64.s
        4.98s      4.98s (flat, cum) 57.24% of Total
            .          .     75:// returns whether AES-NI AND CLMUL-NI are supported
            .          .     76:TEXT ·hasGCMAsm(SB),NOSPLIT,$0
            .          .     77:	XORQ AX, AX
            .          .     78:	INCL AX
            .          .     79:	CPUID
        4.95s      4.95s     80:	MOVQ CX, DX
         20ms       20ms     81:	SHRQ $25, CX
            .          .     82:	SHRQ $1, DX
         10ms       10ms     83:	ANDQ DX, CX
            .          .     84:	ANDQ $1, CX
            .          .     85:	MOVB CX, ret+0(FP)
            .          .     86:	RET
            .          .     87:
            .          .     88:// func aesEncBlock(dst, src *[16]byte, ks []uint32)

From profiling, we see that a large amount of time is spent within
[gcm_amd64.s](https://golang.org/src/crypto/aes/gcm_amd64.s). If the platform
permits, Go implements AES in assembler, making use of [build-in processor
instructions for working with
AES](https://en.wikipedia.org/wiki/AES_instruction_set).

Looking at the hasGCMAsm (has Galois Counter Mode in assembler) function in the
profiler, reported time is likely one instruction off. It's more likely that
line 79 is what takes up more than 50% of the runtime. 

hasGCMAsm stores a 1 in the AX register. A value of 1 in AX while executing the
CPUID instruction causes the CPU to populate the DX register with [various
processor info and feature
flags](https://en.wikipedia.org/wiki/CPUID#EAX=1:_Processor_Info_and_Feature_Bits).
The assembly then checks weather bits 1 and 25 of DX is set. If yes, the
function concludes that the CPU has build-in support for AES. 

Bit 1 indicates support for the [CLMUL instruction
set](https://en.wikipedia.org/wiki/CLMUL_instruction_set), useful for improving
the speed of applications doing block cipher encryption in Galois/Counter Mode
(such as AES). Bit 25 indicates support for the [AES instruction
set](https://en.wikipedia.org/wiki/AES_instruction_set).

It would seem that the Go standard library isn't designed for calling into AES
this many times in a tight loop. In principle, CPUID feature checking could be
moved out of the hot path as support for the CLMUL and AES instruction sets
doesn't change between calls.

## C

The C program makes use of the [libcrypto
API](https://wiki.openssl.org/index.php/Libcrypto_API). libcrypto provides the
fundamental cryptographic routines used by libssl, and is used for base64
decoding and AES decryption.

    rh@xps:~/git/Playground/TrustpilotAesChallenge/C$ gcc program.c -O2 -lcrypto

    rh@xps:~/git/Playground/TrustpilotAesChallenge/C$ /usr/bin/time --verbose ./a.out 
    5 11 14 11 1 7
    Congra�u�a�ions� You found the decryption key. Are you the One?! ;-)
    You're curious and you're up for a challenge! So we like you already.
    If you'd like to work with us at Trustpilot then email us the code you wrote to find the decryption key together with your resume.
    The email is: thereisnospoon@trustpilot.com
    We'll get in touch with you shortly.

    Best regards
    The developers at Trustpilot
        Command being timed: "./a.out"
        User time (seconds): 6.80
        System time (seconds): 0.00
        Percent of CPU this job got: 100%
        Elapsed (wall clock) time (h:mm:ss or m:ss): 0:06.80
        Average shared text size (kbytes): 0
        Average unshared data size (kbytes): 0
        Average stack size (kbytes): 0
        Average total size (kbytes): 0
        Maximum resident set size (kbytes): 3656
        Average resident set size (kbytes): 0
        Major (requiring I/O) page faults: 0
        Minor (reclaiming a frame) page faults: 168
        Voluntary context switches: 1
        Involuntary context switches: 7
        Swaps: 0
        File system inputs: 0
        File system outputs: 0
        Socket messages sent: 0
        Socket messages received: 0
        Signals delivered: 0
        Page size (bytes): 4096
        Exit status: 0

Observe how C uses at maximum 2.4 MB of memory while Go uses 86 MB and
.NET Core around 100 MB. The runtime speed is very impressive at well.

## C#

    rh@xps:~/git/Playground/TrustpilotAesChallenge/CSharp$ /usr/bin/time --verbose dotnet run --configuration release
    5 11 14 11 1 7
    Congra?u?a?ions? You found the decryption key. Are you the One?! ;-)
    You're curious and you're up for a challenge! So we like you already.
    If you'd like to work with us at Trustpilot then email us the code you wrote to find the decryption key together with your resume.
    The email is: thereisnospoon@trustpilot.com
    We'll get in touch with you shortly.

    Best regards
    The developers at Trustpilot

        Command being timed: "dotnet run --configuration release"
        User time (seconds): 37.81
        System time (seconds): 2.96
        Percent of CPU this job got: 106%
        Elapsed (wall clock) time (h:mm:ss or m:ss): 0:38.26
        Average shared text size (kbytes): 0
        Average unshared data size (kbytes): 0
        Average stack size (kbytes): 0
        Average total size (kbytes): 0
        Maximum resident set size (kbytes): 166796
        Average resident set size (kbytes): 0
        Major (requiring I/O) page faults: 0
        Minor (reclaiming a frame) page faults: 181119
        Voluntary context switches: 181444
        Involuntary context switches: 8071
        Swaps: 0
        File system inputs: 0
        File system outputs: 48
        Socket messages sent: 0
        Socket messages received: 0
        Signals delivered: 0
        Page size (bytes): 4096
        Exit status: 0

See also [Improving the performance of .NET crypto
code](https://ndportmann.com/improving-dotnet-crypto-code), optimizing my original implementation from 5m16s to 0m49s.

## F#

No longer maintained.