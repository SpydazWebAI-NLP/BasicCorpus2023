# Corpus Model (.NET VB)

The Corpus Model is a .NET VB code implementation that provides functionalities for working with text corpora. It includes features for vocabulary creation, text encoding/decoding, and generating positional encodings for input tokens.

## Functionality

The Corpus Model consists of the following components:

1. **Vocabulary**: The `Vocabulary` class represents the vocabulary model for the corpus. It allows adding new terms, calculating term frequencies, TF-IDF scores, and checking if a term exists in the vocabulary.

2. **Encode**: The `Encode` class provides methods for encoding text into integer representations based on a given vocabulary. It supports character-level, word-level, and sentence-level encoding.

3. **Decode**: The `Decode` class allows decoding integer representations back into text using a provided vocabulary. It enables the conversion from encoded inputs to their original text forms.

4. **Positional Encoding**: The `PositionalEncoding` class generates positional encodings for input tokens. It adds positional information to the input embeddings to capture the order of tokens in the sequence.

5. **Positional Decoder**: The `PositionalDecoder` class performs decoding of encoded inputs by retrieving the original tokens based on positional encodings. It allows converting encoded inputs back into their original tokenized forms.

## Usage

To use the Corpus Model in your .NET VB project, follow these steps:

1. Create an instance of the `Corpus` class.

2. Initialize the `Vocabulary` with the desired vocabulary type (character, word, or sentence).

3. Optionally, add terms to the vocabulary using the `Vocabulary` class's methods.

4. Use the `Encode` class to encode text into integer representations based on the vocabulary. Choose the appropriate encoding method based on the desired granularity (character, word, or sentence).

5. Utilize the `Decode` class to decode the encoded inputs back into their original text forms using the vocabulary.

6. If positional encodings are required, create an instance of the `PositionalEncoding` class and provide the maximum length and embedding size. Use the `Encode` method to generate positional encodings for input tokens.

7. For decoding positional encodings, create an instance of the `PositionalDecoder` class and provide the maximum length and embedding size. Use the `Decode` method to retrieve the original tokens based on the positional encodings.

## Examples

Here are a few examples to illustrate the usage of the Corpus Model:

## Creating a Vocabulary:
   ```vb
   Dim vocab As New Corpus.Vocabulary()
   vocab.AddNew("cat")
   vocab.AddNew("dog")


## Encoding and Decoding Text:
   
     ```vb
      Dim encodedText As List(Of Integer) = Corpus.Encode.Encode_Text("I have a cat", vocab, Corpus.VocabularyType.Word)
      Dim decodedText As String = Corpus.Decode.Decode_Text(encodedText, vocab)

## Generating Positional Encodings:


    ```vb
    Dim positionalEncoder As New Corpus.PositionalEncoding(maxLength:=10, embeddingSize:=16)
    Dim encodedTokens As List(Of List(Of Double)) = Corpus.Encode.Encode_Text("I have a cat", vocab, Corpus.VocabularyType.Word)
    Dim positionalEncodings As List(Of List(Of Double)) = positionalEncoder.Encode(encodedTokens)

## Decoding Positional Encodings:

    ```vb
    Dim positionalDecoder As New Corpus.PositionalDecoder(maxLength:=10, embeddingSize:=16)
    Dim decodedTokens As List(Of String) = positionalDecoder.Decode(positionalEncodings)

## Note
The provided code assumes a fixed vocabulary and demonstrates basic functionality. You may need to modify and extend the code to suit your specific requirements.