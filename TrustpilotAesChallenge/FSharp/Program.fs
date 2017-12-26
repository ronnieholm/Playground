namespace TrustpilotChallenge

open System
open System.IO
open System.Security.Cryptography

[<AutoOpen>]
module Cryptography =
    let aes = new AesManaged(BlockSize = 128, KeySize = 256, Padding = PaddingMode.Zeros)

    let decrypt (cipherText: byte[]) key iv =
        let decryptor = aes.CreateDecryptor(key, iv)
        use ms = new MemoryStream(cipherText)
        use cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read)
        use sr = new StreamReader(cs)
        sr.ReadToEnd()

module Program =
    [<EntryPoint>]
    let main args =
        let cipherText = Convert.FromBase64String("yptyoDdVBdQtGhgoePppYHnWyugGmy0j81sf3zBeUXEO/LYRw+2XmVa0/v6YiSy9Kj8gMn/gNu2I7dPmfgSEHPUDJpNpiOWmmW1/jw/Pt29Are5tumWmnfkazcAb23xe7B4ruPZVxUEhfn/IrZPNZdr4cQNrHNgEv2ts8gVFuOBU+p792UPy8/mEIhW5ECppxGIb7Yrpg4w7IYNeFtX5d9W4W1t2e+6PcdcjkBK4a8y1cjEtuQ07RpPChOvLcSzlB/Bg7UKntzorRsn+y/d72qD2QxRzcXgbynCNalF7zaT6pEnwKB4i05fTQw6nB7SU1w2/EvCGlfiyR2Ia08mA0GikqegYA6xG/EAGs3ZJ0aQUGt0YZz0P7uBsQKdmCg7jzzEMHyGZDNGTj0F2dOFHLSOTT2/GGSht8eD/Ae7u/xnJj0bGgAKMtNttGFlNyvKpt2vDDT3Orfk6Jk/rD4CIz6O/Tnt0NkJLucHtIyvBYGtQR4+mhbfUELkczeDSxTXGDLaiU3de6tPaa0/vjzizoUbNFdfkIly/HWINdHoO83E=")
        let iv = Convert.FromBase64String("DkBbcmQo1QH+ed1wTyBynA==")
        let key: byte[] = Array.zeroCreate 32

        for a in [0uy..16uy] do
            for b in [0uy..16uy] do
                for c in [0uy..16uy] do
                    for d in [0uy..16uy] do
                        for e in [0uy..16uy] do
                            for f in [0uy..16uy] do
                                key.[0] <- a
                                key.[1] <- b
                                key.[2] <- c
                                key.[3] <- d
                                key.[4] <- e
                                key.[5] <- f

                                let clearText = decrypt cipherText key iv
                                if clearText.Contains("trust") then
                                    printfn "%d %d %d %d %d %d\n%s" a b c d e f clearText
        0