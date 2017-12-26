package main

import (
	"bytes"
	"crypto/aes"
	"crypto/cipher"
	"encoding/base64"
	"fmt"
	"log"
	"net/http"
	_ "net/http/pprof"
)

func decipher() {
	cipherText, err := base64.StdEncoding.DecodeString("yptyoDdVBdQtGhgoePppYHnWyugGmy0j81sf3zBeUXEO/LYRw+2XmVa0/v6YiSy9Kj8gMn/gNu2I7dPmfgSEHPUDJpNpiOWmmW1/jw/Pt29Are5tumWmnfkazcAb23xe7B4ruPZVxUEhfn/IrZPNZdr4cQNrHNgEv2ts8gVFuOBU+p792UPy8/mEIhW5ECppxGIb7Yrpg4w7IYNeFtX5d9W4W1t2e+6PcdcjkBK4a8y1cjEtuQ07RpPChOvLcSzlB/Bg7UKntzorRsn+y/d72qD2QxRzcXgbynCNalF7zaT6pEnwKB4i05fTQw6nB7SU1w2/EvCGlfiyR2Ia08mA0GikqegYA6xG/EAGs3ZJ0aQUGt0YZz0P7uBsQKdmCg7jzzEMHyGZDNGTj0F2dOFHLSOTT2/GGSht8eD/Ae7u/xnJj0bGgAKMtNttGFlNyvKpt2vDDT3Orfk6Jk/rD4CIz6O/Tnt0NkJLucHtIyvBYGtQR4+mhbfUELkczeDSxTXGDLaiU3de6tPaa0/vjzizoUbNFdfkIly/HWINdHoO83E=")
	if err != nil {
		panic(err)
	}

	iv, err := base64.StdEncoding.DecodeString("DkBbcmQo1QH+ed1wTyBynA==")
	if err != nil {
		panic(err)
	}

	clearText := make([]byte, 1024)
	token := []byte("trust")

	k := make([]byte, 32)
	var a, b, c, d, e, f byte
	for a = 0; a <= 16; a++ {
		for b = 0; b <= 16; b++ {
			for c = 0; c <= 16; c++ {
				for d = 0; d <= 16; d++ {
					for e = 0; e <= 16; e++ {
						for f = 0; f <= 16; f++ {
							k[0], k[1], k[2], k[3], k[4], k[5] = a, b, c, d, e, f
							block, err := aes.NewCipher(k)
							if err != nil {
								panic(err)
							}

							decryptor := cipher.NewCBCDecrypter(block, iv)
							decryptor.CryptBlocks(clearText, cipherText)
							if bytes.Contains(clearText, token) {
								fmt.Printf("%v\n%s", k, clearText)
							}
						}
					}
				}
			}
		}
	}
}

func main() {
	// For pprof profiling only
	go func() {
		log.Println(http.ListenAndServe(":8080", nil))
	}()

	decipher()
}
