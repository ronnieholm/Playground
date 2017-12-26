#include <stdio.h>
#include <string.h>
#include <openssl/evp.h>
#include <openssl/aes.h>
#include <openssl/err.h>

typedef unsigned char byte;

// Start: adapted from https://gist.github.com/barrysteyn/4409525
int calcDecodeLength(const char* b64input) {
  int len = strlen(b64input);
  int padding;

  if (b64input[len-1] == '=' && b64input[len-2] == '=') {
    padding = 2;
  }
  else if (b64input[len-1] == '=') {
    padding = 1;
  }

  return (int)len*0.75 - padding;
}

int base64Decode(byte* b64message, byte** buffer) { //Decodes a base64 encoded string
  BIO *bio, *b64;
  int decodeLen = calcDecodeLength(b64message);
  int len = 0;

  *buffer = (byte*)malloc(decodeLen+1);
  FILE* stream = fmemopen(b64message, strlen(b64message), "r");

  b64 = BIO_new(BIO_f_base64());
  bio = BIO_new_fp(stream, BIO_NOCLOSE);
  bio = BIO_push(b64, bio);
  BIO_set_flags(bio, BIO_FLAGS_BASE64_NO_NL);
  len = BIO_read(bio, *buffer, strlen(b64message));
  (*buffer)[len] = '\0';

  BIO_free_all(bio);
  fclose(stream);
  return 0;
}
// End: adapted from https://gist.github.com/barrysteyn/4409525

void handleErrors() {
  ERR_print_errors_fp(stderr);
  abort();
}

int decrypt(byte *cipherText, int cipherText_len, byte *key, byte *iv, byte *plainText) {
  EVP_CIPHER_CTX *ctx;
  int len;
  int plaintext_len;

  if(!(ctx = EVP_CIPHER_CTX_new())) {
    handleErrors();
  }

  if(1 != EVP_DecryptInit_ex(ctx, EVP_aes_256_cbc(), NULL, key, iv)) {
    handleErrors();
  }

  // PKCS padding is used by default:
  // https://wiki.openssl.org/index.php/EVP_Symmetric_Encryption_and_Decryption#Padding.
  // We must switch to zero padding to match the encrypted message.
  EVP_CIPHER_CTX_set_padding(ctx, 0);
  
  // Provide the message to be decrypted, and obtain the plaintext
  // output.  EVP_DecryptUpdate can be called multiple times if
  // necessary.
  if(1 != EVP_DecryptUpdate(ctx, plainText, &len, cipherText, cipherText_len)) {    
    handleErrors();
  }
  plaintext_len = len;

  // Finalisz the decryption. Further plaintext bytes may be written
  // at this stage.
  if(1 != EVP_DecryptFinal_ex(ctx, plainText + len, &len)) {
    handleErrors();
  }
  plaintext_len += len;

  EVP_CIPHER_CTX_free(ctx);
  return plaintext_len;
}

void decrypt_mine() {
  ERR_load_crypto_strings();
  
  byte* cipherText;
  base64Decode("yptyoDdVBdQtGhgoePppYHnWyugGmy0j81sf3zBeUXEO/LYRw+2XmVa0/v6YiSy9Kj8gMn/gNu2I7dPmfgSEHPUDJpNpiOWmmW1/jw/Pt29Are5tumWmnfkazcAb23xe7B4ruPZVxUEhfn/IrZPNZdr4cQNrHNgEv2ts8gVFuOBU+p792UPy8/mEIhW5ECppxGIb7Yrpg4w7IYNeFtX5d9W4W1t2e+6PcdcjkBK4a8y1cjEtuQ07RpPChOvLcSzlB/Bg7UKntzorRsn+y/d72qD2QxRzcXgbynCNalF7zaT6pEnwKB4i05fTQw6nB7SU1w2/EvCGlfiyR2Ia08mA0GikqegYA6xG/EAGs3ZJ0aQUGt0YZz0P7uBsQKdmCg7jzzEMHyGZDNGTj0F2dOFHLSOTT2/GGSht8eD/Ae7u/xnJj0bGgAKMtNttGFlNyvKpt2vDDT3Orfk6Jk/rD4CIz6O/Tnt0NkJLucHtIyvBYGtQR4+mhbfUELkczeDSxTXGDLaiU3de6tPaa0/vjzizoUbNFdfkIly/HWINdHoO83E=", &cipherText);
 
  byte* iv;
  base64Decode("DkBbcmQo1QH+ed1wTyBynA==", &iv);
 
  byte clearText[1024] = {};
  byte key[32] = {};
  
  for (byte a = 0; a <= 16; a++) {
    for (byte b = 0; b <= 16; b++) {
      for (byte c = 0; c <= 16; c++) {
	for (byte d = 0; d <= 16; d++) {
	  for (byte e = 0; e <= 16; e++) {
	    for (byte f = 0; f <= 16; f++) {
	      key[0] = a;
	      key[1] = b;
	      key[2] = c;
	      key[3] = d;
	      key[4] = e;
	      key[5] = f;
	      
	      decrypt(cipherText, 416, key, iv, clearText);
	      if (strstr(clearText, "trust") != NULL) {
	        printf("%d %d %d %d %d %d\n%s\n", a, b, c, d, e, f, clearText);
	      }
	    }
	  }	  
	}
      }
    }
  }

  free(cipherText);
  free(iv);
  EVP_cleanup();
  CRYPTO_cleanup_all_ex_data();
  ERR_free_strings();
}

int main() { 
  decrypt_mine();
 
}
