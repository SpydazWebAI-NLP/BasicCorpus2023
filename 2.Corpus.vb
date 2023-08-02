Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Web.Script.Serialization

Namespace Basic_NLP
    Public Module Helper
        ''' <summary>
        ''' Outputs Structure to Jason(JavaScriptSerializer)
        ''' </summary>
        ''' <returns></returns>
        <Runtime.CompilerServices.Extension()>
        Public Function ToJson(ByRef iObject As Object) As String
            Dim Converter As New JavaScriptSerializer
            Return Converter.Serialize(iObject)
        End Function
        Function CalculateWordOverlap(tokens1 As String(), tokens2 As String()) As Integer
            Dim overlap As Integer = 0

            ' Compare each token in sentence 1 with tokens in sentence 2
            For Each token1 As String In tokens1
                For Each token2 As String In tokens2
                    ' If the tokens match, increment the overlap count
                    If token1.ToLower() = token2.ToLower() Then
                        overlap += 1
                        Exit For ' No need to check further tokens in sentence 2
                    End If
                Next
            Next

            Return overlap
        End Function
        Function DetermineEntailment(overlap As Integer) As Boolean
            ' Set a threshold for entailment
            Dim threshold As Integer = 2

            ' Determine entailment based on overlap
            Return overlap >= threshold
        End Function
        Public Function CalculateCosineSimilarity(sentence1 As String, sentence2 As String) As Double
            ' Calculate the cosine similarity between two sentences
            Dim words1 As String() = sentence1.Split(" "c)
            Dim words2 As String() = sentence2.Split(" "c)

            Dim intersection As Integer = words1.Intersect(words2).Count()
            Dim similarity As Double = intersection / Math.Sqrt(words1.Length * words2.Length)
            Return similarity
        End Function

        Public Sub Program_CALC_SIMULARITY()
            ' Example sentences
            Dim sentence1 As String = "The cat is on the mat."
            Dim sentence2 As String = "The mat has a cat."

            ' Tokenize the sentences
            Dim tokens1 As String() = sentence1.Split(" "c)
            Dim tokens2 As String() = sentence2.Split(" "c)

            ' Calculate the word overlap
            Dim overlap As Integer = CalculateWordOverlap(tokens1, tokens2)

            ' Determine entailment based on overlap
            Dim entailment As Boolean = DetermineEntailment(overlap)

            ' Display the results
            Console.WriteLine("Sentence 1: " & sentence1)
            Console.WriteLine("Sentence 2: " & sentence2)
            Console.WriteLine("Word Overlap: " & overlap)
            Console.WriteLine("Entailment: " & entailment)
            Console.ReadLine()
        End Sub
    End Module
    Public Structure Entity
        Public Property EndIndex As Integer
        Public Property StartIndex As Integer
        Public Property Type As String
        Public Property Value As String
        Public Shared Function DetectEntitys(ByRef text As String, EntityList As List(Of Entity)) As List(Of Entity)
            Dim detectedEntitys As New List(Of Entity)()

            ' Perform entity detection logic here
            For Each item In EntityList
                If text.Contains(item.Value) Then
                    detectedEntitys.Add(item)
                End If
            Next

            Return detectedEntitys
        End Function
    End Structure
    Public Class CorpusReader
        Public Class ModelCorpusReader
            Private corpusRoot As String
            Private corpusFiles As List(Of String)
            Private categoryMap As Dictionary(Of String, List(Of String))

            Public Sub New(corpusRootPath As String)
                corpusRoot = corpusRootPath
                corpusFiles = New List(Of String)()
                categoryMap = New Dictionary(Of String, List(Of String))
                LoadCorpusFiles()
            End Sub

            Private Sub LoadCorpusFiles()
                corpusFiles.Clear()
                If Directory.Exists(corpusRoot) Then
                    corpusFiles.AddRange(Directory.GetFiles(corpusRoot))
                End If
            End Sub

            Public Sub AddCategory(category As String, keywords As List(Of String))
                If Not categoryMap.ContainsKey(category) Then
                    categoryMap.Add(category, keywords)
                Else
                    categoryMap(category).AddRange(keywords)
                End If
            End Sub

            Public Function CategorizeDocument(document As String) As List(Of String)
                Dim categories As New List(Of String)()

                For Each category As KeyValuePair(Of String, List(Of String)) In categoryMap
                    Dim categoryKeywords As List(Of String) = category.Value
                    For Each keyword As String In categoryKeywords
                        If document.Contains(keyword) Then
                            categories.Add(category.Key)
                            Exit For
                        End If
                    Next
                Next

                Return categories
            End Function

            Public Function TaggedSentences() As List(Of List(Of Tuple(Of String, String)))
                Dim itaggedSentences As New List(Of List(Of Tuple(Of String, String)))()

                For Each file As String In corpusFiles
                    Dim taggedSentencesInFile As New List(Of Tuple(Of String, String))()

                    Using reader As New StreamReader(file)
                        While Not reader.EndOfStream
                            Dim line As String = reader.ReadLine()
                            Dim wordsTags As String() = line.Split(" ")

                            For Each wordTag As String In wordsTags
                                Dim parts As String() = wordTag.Split("/")
                                If parts.Length = 2 Then
                                    Dim word As String = parts(0)
                                    Dim tag As String = parts(1)
                                    taggedSentencesInFile.Add(New Tuple(Of String, String)(word, tag))
                                End If
                            Next
                        End While
                    End Using

                    itaggedSentences.Add(taggedSentencesInFile)
                Next

                Return itaggedSentences
            End Function

            Public Function GetWordsFromWordList(wordListFilePath As String) As List(Of String)
                Dim wordList As New List(Of String)()

                Using reader As New StreamReader(wordListFilePath)
                    While Not reader.EndOfStream
                        Dim line As String = reader.ReadLine()
                        If Not String.IsNullOrEmpty(line) Then
                            wordList.Add(line.Trim())
                        End If
                    End While
                End Using

                Return wordList
            End Function
            ' Usage Example:
            Public Shared Sub Main()
                ' Assuming you have a corpus directory with tagged files and a wordlist file
                Dim corpusRootPath As String = "path/to/corpus"
                Dim wordlistFilePath As String = "path/to/wordlist.txt"

                ' Create an instance of the ModelCorpusReader
                Dim corpusReader As New ModelCorpusReader(corpusRootPath)

                ' Add categories and their associated keywords
                corpusReader.AddCategory("Sports", New List(Of String) From {"football", "basketball", "tennis"})
                corpusReader.AddCategory("Technology", New List(Of String) From {"computer", "software", "internet"})
                corpusReader.AddCategory("Politics", New List(Of String) From {"government", "election", "policy"})

                ' Retrieve tagged sentences from the corpus
                Dim taggedSentences As List(Of List(Of Tuple(Of String, String))) = corpusReader.TaggedSentences()

                ' Print the tagged sentences
                For Each sentence As List(Of Tuple(Of String, String)) In taggedSentences
                    For Each wordTag As Tuple(Of String, String) In sentence
                        Console.WriteLine("Word: " & wordTag.Item1 & ", Tag: " & wordTag.Item2)
                    Next
                    Console.WriteLine()
                Next

                ' Retrieve words from the wordlist file
                Dim wordList As List(Of String) = corpusReader.GetWordsFromWordList(wordlistFilePath)

                ' Print the words
                For Each word As String In wordList
                    Console.WriteLine(word)
                Next

                ' Assuming you have a document for categorization
                Dim document As String = "I love playing basketball and football."

                ' Categorize the document
                Dim categories As List(Of String) = corpusReader.CategorizeDocument(document)

                ' Print the categories
                For Each category As String In categories
                    Console.WriteLine(category)
                Next

                ' Rest of your code...
            End Sub


        End Class
        Public Class CorpusCreator
            Public Vocabulary As New List(Of String)
            Public maxSequenceLength As Integer = 0

            Public Sub New(vocabulary As List(Of String), maxSeqLength As Integer)
                If vocabulary Is Nothing Then
                    Throw New ArgumentNullException(NameOf(vocabulary))
                End If

                Me.Vocabulary = vocabulary
                Me.maxSequenceLength = maxSeqLength
            End Sub

            ''' <summary>
            ''' Creates batches of data for training.
            ''' </summary>
            ''' <param name="trainingData">The training data as a list of string sequences.</param>
            ''' <param name="batchSize">The size of each batch.</param>
            Public Sub CreateData(ByRef trainingData As List(Of List(Of String)), ByRef batchSize As Integer)
                For batchStart As Integer = 0 To trainingData.Count - 1 Step batchSize
                    Dim batchEnd As Integer = Math.Min(batchStart + batchSize - 1, trainingData.Count - 1)
                    Dim batchInputs As List(Of List(Of Integer)) = GetBatchInputs(trainingData, batchStart, batchEnd)
                    Dim batchTargets As List(Of List(Of Integer)) = GetBatchTargets(trainingData, batchStart, batchEnd)

                    ' Perform further operations on the batches
                Next

                ' Compute loss
                ' Dim loss As Double = ComputeLoss(predictions, batchTargets)
            End Sub




            ''' <summary>
            ''' Converts a batch of data from a list of string sequences to a list of integer sequences.
            ''' </summary>
            ''' <param name="data">The input data as a list of string sequences.</param>
            ''' <param name="startIndex">The starting index of the batch.</param>
            ''' <param name="endIndex">The ending index of the batch.</param>
            ''' <returns>A list of integer sequences representing the batch inputs.</returns>
            Public Function GetBatchInputs(data As List(Of List(Of String)),
                                       startIndex As Integer,
                                       endIndex As Integer) As List(Of List(Of Integer))
                Dim batchInputs As New List(Of List(Of Integer))

                For i As Integer = startIndex To endIndex
                    Dim sequence As List(Of String) = data(i)

                    ' Convert words to corresponding indices
                    Dim indices As List(Of Integer) = ConvertWordsToIndices(sequence)

                    ' Pad or truncate sequence to the maximum length
                    indices = PadOrTruncateSequence(indices, maxSequenceLength)

                    ' Add the sequence to the batch
                    batchInputs.Add(indices)
                Next

                Return batchInputs
            End Function

            ''' <summary>
            ''' Converts a batch of data from a list of string sequences to a list of integer sequences as targets.
            ''' </summary>
            ''' <param name="data">The input data as a list of string sequences.</param>
            ''' <param name="startIndex">The starting index of the batch.</param>
            ''' <param name="endIndex">The ending index of the batch.</param>
            ''' <returns>A list of integer sequences representing the batch targets.</returns>
            Public Function GetBatchTargets(data As List(Of List(Of String)), startIndex As Integer, endIndex As Integer) As List(Of List(Of Integer))
                Dim batchTargets As New List(Of List(Of Integer))

                For i As Integer = startIndex To endIndex
                    Dim sequence As List(Of String) = data(i)

                    ' Convert words to corresponding indices
                    Dim indices As List(Of Integer) = ConvertWordsToIndices(sequence)

                    ' Shift the sequence to get the target sequence
                    Dim targetIndices As List(Of Integer) = ShiftSequence(indices)

                    ' Pad or truncate sequence to the maximum length
                    targetIndices = PadOrTruncateSequence(targetIndices, maxSequenceLength)

                    ' Add the target sequence to the batch
                    batchTargets.Add(targetIndices)
                Next

                Return batchTargets
            End Function
            ''' <summary>
            ''' Converts a list of words to a list of corresponding indices based on the vocabulary.
            ''' </summary>
            ''' <param name="words">The list of words to convert.</param>
            ''' <returns>A list of corresponding indices.</returns>
            Private Function ConvertWordsToIndices(words As List(Of String)) As List(Of Integer)
                Dim indices As New List(Of Integer)

                For Each word As String In words
                    If Vocabulary.Contains(word) Then
                        indices.Add(Vocabulary.IndexOf(word))
                    Else
                    End If
                Next

                Return indices
            End Function
            ''' <summary>
            ''' Pads or truncates a sequence to a specified length.
            ''' </summary>
            ''' <param name="sequence">The input sequence.</param>
            ''' <param name="length">The desired length.</param>
            ''' <returns>The padded or truncated sequence.</returns>
            Public Function PadOrTruncateSequence(sequence As List(Of Integer), length As Integer) As List(Of Integer)
                If sequence.Count < length Then
                    ' Pad the sequence with a special padding token
                    sequence.AddRange(Enumerable.Repeat(Vocabulary.IndexOf("PAD"), length - sequence.Count))
                ElseIf sequence.Count > length Then
                    ' Truncate the sequence to the desired length
                    sequence = sequence.GetRange(0, length)
                End If

                Return sequence
            End Function

            ''' <summary>
            ''' Shifts a sequence to the right and adds a special token at the beginning.
            ''' </summary>
            ''' <param name="sequence">The input sequence.</param>
            ''' <returns>The shifted sequence.</returns>
            Public Function ShiftSequence(sequence As List(Of Integer)) As List(Of Integer)
                ' Shifts the sequence to the right and adds a special token at the beginning
                Dim shiftedSequence As New List(Of Integer) From {Vocabulary.IndexOf("START")}

                For i As Integer = 0 To sequence.Count - 1
                    shiftedSequence.Add(sequence(i))
                Next

                Return shiftedSequence
            End Function


        End Class
        Public Class CorpusCategorizer
            Private categoryMap As Dictionary(Of String, List(Of String))

            Public Sub New()
                categoryMap = New Dictionary(Of String, List(Of String))()
            End Sub

            Public Sub AddCategory(category As String, keywords As List(Of String))
                If Not categoryMap.ContainsKey(category) Then
                    categoryMap.Add(category, keywords)
                Else
                    categoryMap(category).AddRange(keywords)
                End If
            End Sub

            Public Function CategorizeDocument(document As String) As List(Of String)
                Dim categories As New List(Of String)()

                For Each category As KeyValuePair(Of String, List(Of String)) In categoryMap
                    Dim categoryKeywords As List(Of String) = category.Value
                    For Each keyword As String In categoryKeywords
                        If document.Contains(keyword) Then
                            categories.Add(category.Key)
                            Exit For
                        End If
                    Next
                Next

                Return categories
            End Function


            ' Usage Example:
            Public Shared Sub Main()
                ' Create an instance of the CorpusCategorizer
                Dim categorizer As New CorpusCategorizer()

                ' Add categories and their associated keywords
                categorizer.AddCategory("Sports", New List(Of String) From {"football", "basketball", "tennis"})
                categorizer.AddCategory("Technology", New List(Of String) From {"computer", "software", "internet"})
                categorizer.AddCategory("Politics", New List(Of String) From {"government", "election", "policy"})

                ' Assuming you have a corpus with multiple documents
                Dim corpus As New List(Of String)()
                corpus.Add("I love playing basketball and football.")
                corpus.Add("Software engineering is my passion.")
                corpus.Add("The government announced new policies.")

                ' Categorize each document in the corpus
                For Each document As String In corpus
                    Dim categories As List(Of String) = categorizer.CategorizeDocument(document.ToLower)
                    Console.WriteLine("Categories for document: " & document)
                    For Each category As String In categories
                        Console.WriteLine("- " & category)
                    Next
                    Console.WriteLine()
                Next
                Console.ReadLine()
                ' Rest of your code...
            End Sub

        End Class
        Public Class TaggedCorpusReader
            Private corpusRoot As String
            Private corpusFiles As List(Of String)

            Public Sub New(corpusRootPath As String)
                corpusRoot = corpusRootPath
                corpusFiles = New List(Of String)
                LoadCorpusFiles()
            End Sub

            Private Sub LoadCorpusFiles()
                corpusFiles.Clear()
                If Directory.Exists(corpusRoot) Then
                    corpusFiles.AddRange(Directory.GetFiles(corpusRoot))
                End If
            End Sub

            Public Function TaggedSentences() As List(Of List(Of Tuple(Of String, String)))
                Dim itaggedSentences As New List(Of List(Of Tuple(Of String, String)))()

                For Each file As String In corpusFiles
                    Dim taggedSentencesInFile As New List(Of Tuple(Of String, String))()

                    Using reader As New StreamReader(file)
                        While Not reader.EndOfStream
                            Dim line As String = reader.ReadLine()
                            Dim wordsTags As String() = line.Split(" ")

                            For Each wordTag As String In wordsTags
                                Dim parts As String() = wordTag.Split("/")
                                If parts.Length = 2 Then
                                    Dim word As String = parts(0)
                                    Dim tag As String = parts(1)
                                    taggedSentencesInFile.Add(New Tuple(Of String, String)(word, tag))
                                End If
                            Next
                        End While
                    End Using

                    itaggedSentences.Add(taggedSentencesInFile)
                Next

                Return itaggedSentences
            End Function

            ' Usage Example:
            Public Sub Main()
                ' Assuming you have a corpus directory with tagged files
                Dim corpusRootPath As String = "path/to/corpus"

                ' Create an instance of the TaggedCorpusReader
                Dim corpusReader As New TaggedCorpusReader(corpusRootPath)

                ' Retrieve tagged sentences from the corpus
                Dim taggedSentences As List(Of List(Of Tuple(Of String, String))) = corpusReader.TaggedSentences()

                ' Print the tagged sentences
                For Each sentence As List(Of Tuple(Of String, String)) In taggedSentences
                    For Each wordTag As Tuple(Of String, String) In sentence
                        Console.WriteLine("Word: " & wordTag.Item1 & ", Tag: " & wordTag.Item2)
                    Next
                    Console.WriteLine()
                Next

                ' Rest of your code...
            End Sub
        End Class
        Public Class WordListCorpusReader
            Private wordList As List(Of String)

            Public Sub New(filePath As String)
                wordList = New List(Of String)()
                ReadWordList(filePath)
            End Sub

            Private Sub ReadWordList(filePath As String)
                Using reader As New StreamReader(filePath)
                    While Not reader.EndOfStream
                        Dim line As String = reader.ReadLine()
                        If Not String.IsNullOrEmpty(line) Then
                            wordList.Add(line.Trim())
                        End If
                    End While
                End Using
            End Sub

            Public Function GetWords() As List(Of String)
                Return wordList
            End Function
            ' Usage Example:
            Public Shared Sub Main()
                ' Assuming you have a wordlist file named 'words.txt' in the same directory
                Dim corpusRoot As String = "."
                Dim wordlistPath As String = Path.Combine(corpusRoot, "wordlist.txt")

                Dim wordlistReader As New WordListCorpusReader(wordlistPath)
                Dim words As List(Of String) = wordlistReader.GetWords()

                For Each word As String In words
                    Console.WriteLine(word)
                Next
                Console.ReadLine()
                ' Rest of your code...
            End Sub


        End Class
    End Class

    ''' <summary>
    ''' Corpus Language Model
    ''' Used to HoldDocuments : a corpus of documents Calculating detecting the 
    ''' known entitys and topics in the model; 
    ''' A known list of Entitys and Topics are required to create this model
    ''' This language model is ideally suited for NER / and other corpus interogations
    ''' 
    ''' </summary>
    Public Class Corpus
        Public Class iCompare

            Public Shared Function GetDistinctWords(text As String) As HashSet(Of String)
                ' Split the text into words and return a HashSet of distinct words
                Dim words() As String = text.Split({" ", ".", ",", ";", ":", "!", "?"}, StringSplitOptions.RemoveEmptyEntries)
                Dim distinctWords As New HashSet(Of String)(words, StringComparer.OrdinalIgnoreCase)

                Return distinctWords
            End Function
            Public Shared Function BuildWordVector(words As HashSet(Of String)) As Dictionary(Of String, Integer)
                Dim wordVector As New Dictionary(Of String, Integer)

                For Each word As String In words
                    If wordVector.ContainsKey(word) Then
                        wordVector(word) += 1
                    Else
                        wordVector(word) = 1
                    End If
                Next

                Return wordVector
            End Function
            '1. Cosine Similarity Calculation:
            '```vb
            Public Shared Function ComputeCosineSimilarity(phrase1 As String, phrase2 As String) As Double
                Dim words1 As HashSet(Of String) = GetDistinctWords(phrase1)
                Dim words2 As HashSet(Of String) = GetDistinctWords(phrase2)

                Dim wordVector1 As Dictionary(Of String, Integer) = BuildWordVector(words1)
                Dim wordVector2 As Dictionary(Of String, Integer) = BuildWordVector(words2)

                Dim dotProduct As Integer = ComputeDotProduct(wordVector1, wordVector2)
                Dim magnitude1 As Double = ComputeVectorMagnitude(wordVector1)
                Dim magnitude2 As Double = ComputeVectorMagnitude(wordVector2)

                ' Compute the cosine similarity as the dot product divided by the product of magnitudes
                Dim similarityScore As Double = dotProduct / (magnitude1 * magnitude2)

                Return similarityScore
            End Function
            Public Shared Function ComputeDotProduct(vector1 As Dictionary(Of String, Integer), vector2 As Dictionary(Of String, Integer)) As Integer
                Dim dotProduct As Integer = 0

                For Each word As String In vector1.Keys
                    If vector2.ContainsKey(word) Then
                        dotProduct += vector1(word) * vector2(word)
                    End If
                Next

                Return dotProduct
            End Function
            '2. Jaccard Similarity Calculation:
            '```vb
            Public Shared Function ComputeJaccardSimilarity(phrase1 As String, phrase2 As String) As Double
                Dim words1 As HashSet(Of String) = GetDistinctWords(phrase1)
                Dim words2 As HashSet(Of String) = GetDistinctWords(phrase2)

                Dim intersectionCount As Integer = words1.Intersect(words2).Count()
                Dim unionCount As Integer = words1.Count + words2.Count - intersectionCount

                ' Compute the Jaccard Similarity as the ratio of intersection count to union count
                Dim similarityScore As Double = intersectionCount / unionCount

                Return similarityScore
            End Function
            Public Shared Function ComputeSimilarityScore(phrase As String, contextLine As String) As Double
                ' Here you can implement your own logic for computing the similarity score between the phrase and the context line.
                ' For simplicity, let's use a basic approach that counts the number of common words between them.

                Dim phraseWords As HashSet(Of String) = GetDistinctWords(phrase)
                Dim contextWords As HashSet(Of String) = GetDistinctWords(contextLine)

                Dim commonWordsCount As Integer = phraseWords.Intersect(contextWords).Count()

                Dim totalWordsCount As Integer = phraseWords.Count + contextWords.Count

                ' Compute the similarity score as the ratio of common words count to total words count
                Dim similarityScore As Double = commonWordsCount / totalWordsCount

                Return similarityScore
            End Function
            Public Shared Function ComputeVectorMagnitude(vector As Dictionary(Of String, Integer)) As Double
                Dim magnitude As Double = 0

                For Each count As Integer In vector.Values
                    magnitude += count * count
                Next

                magnitude = Math.Sqrt(magnitude)

                Return magnitude
            End Function
        End Class
        ''' <summary>
        ''' Used to create NewCorpus - With Or Without a Recognition template
        ''' </summary>
        Public Class ProcessInputAPI
            Private iCurrentOriginalText As String
            Private KnownEntitys As Corpus.Recognition_Data


            Public Sub New(ByRef KnownData As Corpus.Recognition_Data)
                Me.KnownEntitys = KnownData
            End Sub
            Public Sub New()
                KnownEntitys = New Corpus.Recognition_Data
            End Sub
            Public ReadOnly Property CurrentInput As String
                Get
                    Return iCurrentOriginalText
                End Get
            End Property
            Public Function ProcessDocument(ByRef InputText As String) As Corpus
                Dim iCorpus As New Corpus(KnownEntitys)
                iCorpus.AddDocument(InputText)
                Return iCorpus
            End Function
            Public Function ProcessCorpus(ByRef InputText As List(Of String)) As Corpus
                Dim iCorpus As New Corpus(KnownEntitys)
                iCorpus.AddCorpus(InputText)
                Return iCorpus
            End Function

        End Class
        Public Shared Function ExtractSimilarPhrases(text As String, searchPhrase As String, similarityThreshold As Double) As List(Of String)
            Dim result As New List(Of String)()

            Dim sentences() As String = text.Split({".", "!", "?"}, StringSplitOptions.RemoveEmptyEntries)

            For Each sentence As String In sentences
                Dim similarityScore As Double = iCompare.ComputeSimilarityScore(searchPhrase, sentence)

                If similarityScore >= similarityThreshold Then
                    result.Add(sentence)
                End If
            Next

            Return result
        End Function
        Public Shared Function QueryCorpus(question As String, corpus As List(Of String)) As String
            Dim maxScore As Double = Double.MinValue
            Dim bestAnswer As String = ""

            For Each document As String In corpus
                Dim score As Double = iCompare.ComputeSimilarityScore(question, document)

                If score > maxScore Then
                    maxScore = score
                    bestAnswer = document
                End If
            Next

            Return bestAnswer
        End Function

        ''' <summary>
        ''' Returns phrase and surrounding comments and position
        ''' </summary>
        ''' <param name="corpus"></param>
        ''' <param name="phrase"></param>
        ''' <returns></returns>
        Public Shared Function SearchPhraseInCorpus(corpus As List(Of String), phrase As String) As Dictionary(Of String, List(Of String))
            Dim result As New Dictionary(Of String, List(Of String))()

            For i As Integer = 0 To corpus.Count - 1
                Dim document As String = corpus(i)
                Dim lines() As String = document.Split(Environment.NewLine)

                For j As Integer = 0 To lines.Length - 1
                    Dim line As String = lines(j)
                    Dim index As Integer = line.IndexOf(phrase, StringComparison.OrdinalIgnoreCase)

                    While index >= 0
                        Dim context As New List(Of String)()

                        ' Get the surrounding context sentences
                        Dim startLine As Integer = Math.Max(0, j - 1)
                        Dim endLine As Integer = Math.Min(lines.Length - 1, j + 1)

                        For k As Integer = startLine To endLine
                            context.Add(lines(k))
                        Next

                        ' Add the result to the dictionary
                        Dim position As String = $"Document: {i + 1}, Line: {j + 1}, Character: {index + 1}"
                        result(position) = context

                        ' Continue searching for the phrase in the current line
                        index = line.IndexOf(phrase, index + 1, StringComparison.OrdinalIgnoreCase)
                    End While
                Next
            Next

            Return result
        End Function
        ''' <summary>
        ''' Searches for phrases based on simularity ie same words
        ''' </summary>
        ''' <param name="corpus"></param>
        ''' <param name="phrase"></param>
        ''' <param name="similarityThreshold"></param>
        ''' <returns></returns>
        Public Shared Function SearchPhraseInCorpus(corpus As List(Of String), phrase As String, similarityThreshold As Double) As Dictionary(Of String, List(Of String))
            Dim result As New Dictionary(Of String, List(Of String))()

            For i As Integer = 0 To corpus.Count - 1
                Dim document As String = corpus(i)
                Dim lines() As String = document.Split(Environment.NewLine)

                For j As Integer = 0 To lines.Length - 1
                    Dim line As String = lines(j)
                    Dim index As Integer = line.IndexOf(phrase, StringComparison.OrdinalIgnoreCase)

                    While index >= 0
                        Dim context As New List(Of String)()

                        ' Get the surrounding context sentences
                        Dim startLine As Integer = Math.Max(0, j - 1)
                        Dim endLine As Integer = Math.Min(lines.Length - 1, j + 1)

                        For k As Integer = startLine To endLine
                            Dim contextLine As String = lines(k)

                            ' Compute the similarity score between the context line and the phrase
                            Dim similarityScore As Double = iCompare.ComputeSimilarityScore(phrase, contextLine)

                            ' Add the context line only if its similarity score exceeds the threshold
                            If similarityScore >= similarityThreshold Then
                                context.Add(contextLine)
                            End If
                        Next

                        ' Add the result to the dictionary
                        Dim position As String = $"Document: {i + 1}, Line: {j + 1}, Character: {index + 1}"
                        result(position) = context

                        ' Continue searching for the phrase in the current line
                        index = line.IndexOf(phrase, index + 1, StringComparison.OrdinalIgnoreCase)
                    End While
                Next
            Next

            Return result
        End Function

        Public Function ToJson(ByRef iObject As Object) As String
            Dim Converter As New JavaScriptSerializer
            Return Converter.Serialize(iObject)
        End Function
        Public Class Tokenizer
            ''' <summary>
            ''' Normalizes the input string by converting it to lowercase and removing punctuation and extra whitespace.
            ''' </summary>
            ''' <param name="input">The input string.</param>
            ''' <returns>The normalized input string.</returns>
            Public Function NormalizeInput(input As String) As String
                ' Convert to lowercase
                Dim normalizedInput As String = input.ToLower()

                ' Remove punctuation
                normalizedInput = Regex.Replace(normalizedInput, "[^\w\s]", "")

                ' Remove extra whitespace
                normalizedInput = Regex.Replace(normalizedInput, "\s+", " ")

                Return normalizedInput
            End Function

            ''' <summary>
            ''' Tokenizes the input string by character.
            ''' </summary>
            ''' <param name="input">The input string.</param>
            ''' <returns>The list of character tokens.</returns>
            Public Shared Function TokenizeByCharacter(input As String) As List(Of Token)
                Dim tokens As New List(Of Token)

                For i As Integer = 0 To input.Length - 1
                    Dim token As New Token(input(i).ToString())
                    tokens.Add(token)
                Next

                Return tokens
            End Function

            ''' <summary>
            ''' Tokenizes the input string by word.
            ''' </summary>
            ''' <param name="input">The input string.</param>
            ''' <returns>The list of word tokens.</returns>
            Public Shared Function TokenizeByWord(input As String) As List(Of Token)
                Dim tokens As New List(Of Token)
                Dim words As String() = input.Split(" "c)

                For i As Integer = 0 To words.Length - 1
                    Dim token As New Token(words(i))
                    tokens.Add(token)
                Next

                Return tokens
            End Function

            ''' <summary>
            ''' Tokenizes the input string by sentence.
            ''' </summary>
            ''' <param name="input">The input string.</param>
            ''' <returns>The list of sentence tokens.</returns>
            Public Shared Function TokenizeBySentence(input As String) As List(Of Token)
                Dim tokens As New List(Of Token)
                Dim sentences As String() = input.Split("."c)

                For i As Integer = 0 To sentences.Length - 1
                    Dim token As New Token(sentences(i))
                    tokens.Add(token)
                Next

                Return tokens
            End Function

            ''' <summary>
            ''' Tokenizes the input string by whitespace.
            ''' </summary>
            ''' <param name="input">The input string.</param>
            ''' <returns>The list of tokens.</returns>
            Public Shared Function Tokenize(input As String) As List(Of String)
                ' Simple tokenization by splitting on whitespace
                Return New List(Of String)(input.Split({" "c}, StringSplitOptions.RemoveEmptyEntries))
            End Function
            Public Class Token
                ''' <summary>
                ''' Initializes a new instance of the Token class.
                ''' </summary>
                ''' <param name="value">The string value of the token.</param>
                Public Sub New(value As String)
                    If value Is Nothing Then
                        Throw New ArgumentNullException(NameOf(value))
                    End If

                    Me.Value = value
                End Sub

                ''' <summary>
                ''' Initializes a new instance of the Token class with sequence encoding.
                ''' </summary>
                ''' <param name="value">The string value of the token.</param>
                ''' <param name="sequenceEncoding">The sequence encoding value of the token.</param>
                Public Sub New(value As String, sequenceEncoding As Integer)
                    Me.New(value)
                    Me.SequenceEncoding = sequenceEncoding
                End Sub

                ''' <summary>
                ''' Gets or sets the embeddings of the token.
                ''' </summary>
                Public Property Embeddings As List(Of Double)
                ''' <summary>
                ''' Calculates the similarity between this token and the given token.
                ''' </summary>
                ''' <param name="token">The other token.</param>
                ''' <returns>The similarity value between the tokens.</returns>
                Private Function CalculateSimilarity(token As Token) As Double
                    If Embeddings IsNot Nothing AndAlso token.Embeddings IsNot Nothing Then
                        Dim dotProduct As Double = 0.0
                        Dim magnitudeA As Double = 0.0
                        Dim magnitudeB As Double = 0.0

                        For i As Integer = 0 To Embeddings.Count - 1
                            dotProduct += Embeddings(i) * token.Embeddings(i)
                            magnitudeA += Math.Pow(Embeddings(i), 2)
                            magnitudeB += Math.Pow(token.Embeddings(i), 2)
                        Next

                        magnitudeA = Math.Sqrt(magnitudeA)
                        magnitudeB = Math.Sqrt(magnitudeB)

                        If magnitudeA = 0.0 OrElse magnitudeB = 0.0 Then
                            Return 0.0
                        Else
                            Return dotProduct / (magnitudeA * magnitudeB)
                        End If
                    Else
                        Return 0.0
                    End If
                End Function
                ''' <summary>
                ''' Gets or sets the string value of the token.
                ''' </summary>
                Public Property Value As String

                ''' <summary>
                ''' Gets or sets the sequence encoding value of the token.
                ''' </summary>
                Public Property SequenceEncoding As Integer

                ''' <summary>
                ''' Gets or sets the positional encoding value of the token.
                ''' </summary>
                Public Property PositionalEncoding As Integer

                ''' <summary>
                ''' Gets or sets the frequency of the token in the language model corpus.
                ''' </summary>
                Public Property Frequency As Double

                ''' <summary>
                ''' Gets or sets the embedding vector of the token.
                ''' </summary>
                Public Property Embedding As Double
                Public Function CalculateSelfAttention(tokens As List(Of Token)) As Double
                    Dim total As Double = 0.0
                    For Each token As Token In tokens
                        total += CalcSimilarity(token)
                    Next
                    Return Math.Log(Math.Sqrt(total))
                End Function

                Private Function CalcSimilarity(token As Token) As Double
                    If Embeddings IsNot Nothing AndAlso token.Embeddings IsNot Nothing Then
                        Dim dotProduct As Double = 0.0
                        For i As Integer = 0 To Embeddings.Count - 1
                            dotProduct += Embeddings(i) * token.Embeddings(i)
                        Next
                        Return dotProduct
                    End If
                    Return 0.0
                End Function

                ''' <summary>
                ''' Calculates the self-attention of the token within the given list of tokens.
                ''' </summary>
                ''' <param name="tokens">The list of tokens.</param>
                ''' <returns>The self-attention value of the token.</returns>
                Public Function CalculateAttention(tokens As List(Of Token)) As Double
                    Dim qVector As List(Of Double) = Me.Embeddings
                    Dim kMatrix As New List(Of Double)
                    Dim vMatrix As New List(Of Double)

                    ' Create matrices K and V
                    For Each token In tokens
                        kMatrix.Add(token.Embedding)
                        vMatrix.Add(token.Embedding)
                    Next

                    ' Compute self-attention
                    Dim attention As Double = 0.0
                    Dim sqrtKLength As Double = Math.Sqrt(kMatrix(0))

                    For i As Integer = 0 To kMatrix.Count - 1
                        Dim kVector As List(Of Double) = kMatrix
                        Dim dotProduct As Double = 0.0

                        ' Check vector dimensions
                        If qVector.Count = kVector.Count Then
                            For j As Integer = 0 To qVector.Count - 1
                                dotProduct += qVector(j) * kVector(j)
                            Next

                            dotProduct /= sqrtKLength
                            attention += dotProduct * vMatrix(i) ' We consider only the first element of the value vector for simplicity
                        Else
                            ' Handle case when vector dimensions do not match
                            Console.WriteLine("Vector dimensions do not match.")
                        End If
                    Next

                    Return attention
                End Function
            End Class
        End Class

        ''' <summary>
        ''' An array of characters (. ! ?) used to tokenize sentences.
        ''' </summary>
        Public Shared ReadOnly SentenceEndMarkers As Char() = {".", "!", "?"}

        Public CorpusContext As List(Of Vocabulary.FeatureContext)

        ''' <summary>
        ''' A list of strings representing the documents in the corpus.
        ''' </summary>
        Public CorpusDocs As List(Of String)

        ''' <summary>
        ''' A string representing the concatenated text of all documents in the corpus.
        ''' </summary>
        Public CorpusText As String

        ''' <summary>
        '''  A list of unique words in the corpus.
        ''' </summary>
        Public CorpusUniqueWords As List(Of String)

        ''' <summary>
        ''' TotalWords in Corpus
        ''' </summary>
        Public ReadOnly Property CorpusWordcount As Integer
            Get
                Return GetWordCount()
            End Get
        End Property

        ''' <summary>
        '''  A list of Document objects representing individual documents in the corpus.
        ''' </summary>
        Public Documents As List(Of Document)

        ''' <summary>
        ''' A list of Entity structures representing detected entities in the corpus.
        ''' </summary>
        Public Entitys As List(Of Entity)

        ''' <summary>
        ''' A Vocabulary object representing the language model.
        ''' </summary>
        Public Langmodel As Vocabulary

        ''' <summary>
        ''' A Recognition_Data structure representing named entity recognition data.
        ''' </summary>
        Public NER As Recognition_Data

        ''' <summary>
        ''' A list of Topic structures representing detected topics in the corpus.
        ''' </summary>
        Public Topics As List(Of Topic)

        ''' <summary>
        ''' Initializes a new instance of the Corpus class.
        ''' </summary>
        ''' <param name="data">The recognition data for entity and topic detection.</param>
        Public Sub New(ByVal data As Recognition_Data)
            NER = data
            Documents = New List(Of Document)
            Entitys = New List(Of Entity)
            Topics = New List(Of Topic)
            CorpusDocs = New List(Of String)
            CorpusUniqueWords = New List(Of String)
            CorpusText = String.Empty

            Langmodel = New Vocabulary
        End Sub

        ''' <summary>
        ''' type of sentence
        ''' </summary>
        Public Enum SentenceType
            Unknown = 0
            Declaritive = 1
            Interogative = 2
            Exclamitory = 3
            Conditional = 4
            Inference = 5
            Imperitive = 6
        End Enum




        ''' <summary>
        ''' Processes the text by removing unwanted characters, converting to lowercase, and removing extra whitespace.
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        Public Shared Function ProcessText(ByRef text As String) As String
            ' Remove unwanted characters
            Dim processedText As String = Regex.Replace(text, "[^a-zA-Z0-9\s]", "")

            ' Convert to lowercase
            processedText = processedText.ToLower()

            ' Remove extra whitespace
            processedText = Regex.Replace(processedText, "\s+", " ")

            Return processedText
        End Function

        ''' <summary>
        ''' Adds a corpus of documents to the existing corpus.
        ''' </summary>
        ''' <param name="docs"></param>
        ''' <returns></returns>
        Public Function AddCorpus(ByRef docs As List(Of String)) As Corpus

            'Add aCorpus of documents to the corpus

            For Each item In docs

                AddDocument(item)

            Next
            UpdateContext()
            Return Me

        End Function

        ''' <summary>
        ''' Adds a document to the corpus and updates the corpus properties.
        ''' </summary>
        ''' <param name="Text"></param>
        Public Sub AddDocument(ByRef Text As String)
            Dim Doc As New Document(Text)
            Documents.Add(Doc.AddDocument(ProcessText(Text)))
            'Update Corpus
            CorpusDocs.Add(ProcessText(Text))

            CorpusUniqueWords = GetUniqueWords()

            Dim iText As String = ""
            For Each item In Documents
                iText &= item.ProcessedText & vbNewLine

            Next
            CorpusText = iText

            '' corpus entitys and topics
            Doc.Entitys = Entity.DetectEntitys(Doc.ProcessedText, NER.Entitys)
            Doc.Topics = Topic.DetectTopics(Doc.ProcessedText, NER.Topics)
            Entitys.AddRange(Doc.Entitys)
            Entitys = Entitys

            Topics.AddRange(Doc.Topics)
            Topics = Topics
            'Update VocabModel

            Dim Wrds = Text.Split(" ")

            For Each item In Wrds
                Langmodel.AddNew(item, CorpusDocs)
            Next
        End Sub

        ''' <summary>
        ''' Retrieves the list of unique words in the corpus.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetUniqueWords() As List(Of String)
            Dim lst As New List(Of String)
            For Each item In Documents
                lst.AddRange(item.UniqueWords)
            Next
            Return lst
        End Function

        ''' <summary>
        ''' Retrieves the total word count in the corpus.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetWordCount() As Integer
            Dim count As Integer = 0
            For Each item In Documents
                count += item.WordCount
            Next
            Return count
        End Function

        ''' <summary>
        ''' Updates the Features in the model (each document context)
        ''' by the topics discovered in the text, updating the individual documents and adding the
        ''' feature context to the corpus context
        ''' </summary>
        Private Sub UpdateContext()
            CorpusContext = New List(Of Vocabulary.FeatureContext)
            For Each Topic In Topics.Distinct
                For Each doc In Documents
                    Dim Context = Vocabulary.FeatureContext.GetDocumentContext(Langmodel, doc, Topic.Topic)
                    doc.Features.Add(Context)
                    CorpusContext.Add(Context)
                Next
            Next

        End Sub

        ''' <summary>
        ''' Represents an individual document in the corpus. It contains properties such as word count, processed text, sentences, topics, etc.
        ''' </summary>
        Public Structure Document

            Public ReadOnly Property WordCount As Integer
                Get
                    Return GetWordCount()
                End Get
            End Property
            Private Function GetWordCount() As Integer
                Dim Str = Functs.TokenizeWords(OriginalText)
                Return Str.Count
            End Function
            '''' <summary>
            '''' COntains the Vocabulary for this document
            '''' </summary>
            Public DocumentVocabulary As Vocabulary

            Public Entitys As List(Of Entity)

            ''' <summary>
            ''' Context can be updated by the corpus owner as required, these contexts
            ''' can be used to score the document and provided higher embedding values
            ''' </summary>
            Public Features As List(Of Vocabulary.FeatureContext)

            ''' <summary>
            ''' Preserve original
            ''' </summary>
            Public OriginalText As String

            ''' <summary>
            ''' Cleaned Text
            ''' </summary>
            Public ProcessedText As String

            ''' <summary>
            ''' Sentences within Text
            ''' </summary>
            Public Sentences As List(Of Sentence)

            Public Topics As List(Of Topic)
            Public TopWords As List(Of String)
            Public UniqueWords As List(Of String)

            Public Sub New(ByRef originalText As String)

                Me.OriginalText = originalText
                Topics = New List(Of Topic)
                TopWords = New List(Of String)
                UniqueWords = New List(Of String)
                Sentences = New List(Of Sentence)
                DocumentVocabulary = New Vocabulary
                Entitys = New List(Of Entity)
            End Sub

            Public Function AddDocument(ByRef Text As String) As Document
                OriginalText = Text
                'Remove unwanted symbols
                ProcessedText = ProcessText(Text)

                Dim Sents As List(Of String) = Text.Split(".").ToList
                Dim Count As Integer = 0
                For Each item In Sents
                    Count += 1
                    Dim Sent As New Sentence(item)
                    Me.Sentences.Add(Sent.AddSentence(item, Count))
                Next
                UniqueWords = Corpus.Functs.GetUniqueWordsInText(ProcessedText)
                Dim IDocs As New List(Of String)
                'Adds only its-self to its own personal corpus vocabulary(document Specific)
                IDocs.Add(ProcessedText)
                For Each item In UniqueWords
                    DocumentVocabulary.AddNew(item, IDocs)
                Next
                TopWords = Corpus.Functs.GetTopWordsInText(ProcessedText)

                Return Me
            End Function

            Public Structure Sentence

                Public Clauses As List(Of Clause)

                Public Entitys As List(Of Entity)

                Public OriginalSentence As String

                Public Position As Integer

                Public ProcessedSentence As String

                Public UniqueWords As List(Of String)

                Private iSentencetype As SentenceType

                Public Sub New(originalSentence As String)
                    Me.New()
                    Me.OriginalSentence = originalSentence
                    Clauses = New List(Of Clause)
                    Entitys = New List(Of Entity)
                    UniqueWords = New List(Of String)
                End Sub

                Public ReadOnly Property ClauseCount As Integer
                    Get
                        Return Clauses.Count
                    End Get

                End Property
                Public ReadOnly Property SentenceType As String
                    Get
                        Select Case iSentencetype
                            Case Corpus.SentenceType.Conditional
                                Return "Conditional"
                            Case Corpus.SentenceType.Declaritive
                                Return "Declarative"
                            Case Corpus.SentenceType.Exclamitory
                                Return "exclamatory"
                            Case Corpus.SentenceType.Imperitive
                                Return "imperative"
                            Case Corpus.SentenceType.Inference
                                Return "inference"
                            Case Corpus.SentenceType.Interogative
                                Return "interrogative"
                            Case Corpus.SentenceType.Unknown
                                Return "unknown"
                            Case Else
                                Return "unknown"
                        End Select
                    End Get
                End Property

                Public ReadOnly Property WordCount As Integer
                    Get
                        Return GetWordCount(ProcessedSentence)
                    End Get
                End Property

                Public Shared Function GetClauses(ByRef Text As String) As List(Of Clause)
                    Dim clauses As New List(Of Clause)

                    '

                    If Text.Contains(",") Then
                        Dim iClauses As List(Of String) = Text.Split(",").ToList
                        For Each item In iClauses
                            Dim Iclause As New Clause
                            Iclause.Text = item
                            Iclause.ClauseSeperator = ","
                            Dim Words = Functs.TokenizeWords(Iclause.Text)
                            Dim count As Integer = 0
                            For Each wrd In Words
                                count += 1
                                Iclause.Words.Add(New Clause.Word(wrd, count))

                            Next

                            clauses.Add(Iclause)

                        Next
                    Else

                        'Add detect end punctuation use for

                        Dim Iclause As New Clause
                        Iclause.Words = New List(Of Clause.Word)
                        Iclause.Text = Text
                        'Use end punctuation
                        Iclause.ClauseSeperator = "."
                        Dim Words = Functs.TokenizeWords(Iclause.Text)
                        Dim count As Integer = 0
                        If Words.Count > 0 Then
                            For Each wrd In Words

                                count += 1
                                Iclause.Words.Add(New Clause.Word(wrd, count))

                            Next
                        End If
                        clauses.Add(Iclause)

                    End If
                    Return clauses
                End Function

                Public Function AddSentence(ByRef text As String, ByRef iPosition As Integer) As Sentence
                    OriginalSentence = text
                    ProcessedSentence = ProcessText(text)
                    Clauses = GetClauses(ProcessedSentence)
                    UniqueWords = Corpus.Functs.GetUniqueWordsInText(ProcessedSentence)

                    Position = iPosition
                    Return Me
                End Function

                Private Function GetWordCount(ByRef Text As String) As Integer
                    Dim Str = Functs.TokenizeWords(Text)
                    Return Str.Count
                End Function

                ''' <summary>
                ''' Represents a clause within a sentence. It contains properties such as text, word count, words, etc.
                ''' </summary>
                Public Structure Clause
                    ''' <summary>
                    ''' Independent Clause / Dependant Clause
                    ''' </summary>
                    Public Clause As String

                    Public ClauseSeperator As String
                    Public ClauseType As SentenceType
                    ''' <summary>
                    ''' Note: if = "." then declarative, = "?" Question = "!" Exclamitory
                    ''' </summary>
                    Public EndPunctuation As String

                    Public Text As String
                    Public Words As List(Of Clause.Word)
                    Private mLearningPattern As String

                    Private mPredicate As String

                    Private mSubjectA As String

                    Private mSubjectB As String

                    ''' <summary>
                    ''' the learning pattern locates the Subjects in the sentence A# sat on #b
                    ''' </summary>
                    ''' <returns></returns>
                    Public Property LearningPattern As String
                        Get
                            Return mLearningPattern
                        End Get
                        Set(value As String)
                            mLearningPattern = value
                        End Set
                    End Property

                    ''' <summary>
                    ''' Predicate / Linking verb / Concept (Sat on) (is sitting on) (AtLocation) this is the
                    ''' dividing content in the sentence
                    ''' </summary>
                    ''' <returns></returns>
                    Public Property Predicate As String
                        Get
                            Return mPredicate
                        End Get
                        Set(value As String)
                            mPredicate = value
                        End Set
                    End Property

                    ''' <summary>
                    ''' First detected subject (the Cat)
                    ''' </summary>
                    ''' <returns></returns>
                    Public Property SubjectA As String
                        Get
                            Return mSubjectA
                        End Get
                        Set(value As String)
                            mSubjectA = value
                        End Set
                    End Property

                    ''' <summary>
                    ''' Second detected subject / Object (the mat)
                    ''' </summary>
                    ''' <returns></returns>
                    Public Property SubjectB As String
                        Get
                            Return mSubjectB
                        End Get
                        Set(value As String)
                            mSubjectB = value
                        End Set
                    End Property

                    Public ReadOnly Property WordCount As Integer
                        Get
                            Return Words.Count
                        End Get

                    End Property
                    ''' <summary>
                    ''' Represents a word in the text
                    ''' </summary>
                    Public Structure Word

                        ''' <summary>
                        ''' Position of word in Sentence/Document
                        ''' </summary>
                        Public Position As Integer

                        ''' <summary>
                        ''' Word
                        ''' </summary>
                        Public text As String


                        Public Sub New(word As String, position As Integer)
                            If word Is Nothing Then
                                Throw New ArgumentNullException(NameOf(word))
                            End If

                            Me.text = word
                            Me.Position = position

                        End Sub

                    End Structure

                End Structure

            End Structure

        End Structure

        ''' <summary>
        ''' NER Data held(known) by the corpus
        ''' </summary>
        Public Class Recognition_Data
            Public Entitys As List(Of Entity)
            Public Topics As List(Of Topic)
            Public Sub New()
                Entitys = New List(Of Entity)
                Topics = New List(Of Topic)
            End Sub
        End Class

        Public Structure Term
            Public DocNumber As List(Of Integer)
            ''' <summary>
            ''' Term Frequency
            ''' </summary>
            Dim Freq As Integer
            ''' <summary>
            ''' Inverse Document Frequency
            ''' </summary>
            Dim IDF As Double
            ''' <summary>
            ''' Value
            ''' </summary>
            Dim Term As String


        End Structure

        ''' <summary>
        ''' Represents a topic detected in the text. It has properties for the keyword and topic itself.
        ''' </summary>
        Public Structure Topic
            Public Keyword As String
            Public Topic As String

            Public Shared Function DetectTopics(ByRef text As String, TopicList As List(Of Topic)) As List(Of Topic)
                Dim detectedTopics As New List(Of Topic)()
                For Each item In TopicList
                    If text.ToLower.Contains(item.Keyword) Then
                        detectedTopics.Add(item)
                    End If
                Next

                Return detectedTopics
            End Function

        End Structure

        Public Class Functs

            ''' <summary>
            ''' Returns the top words in a given text
            ''' </summary>
            ''' <param name="text"></param>
            ''' <returns></returns>
            Public Shared Function GetTopWordsInText(ByRef text As String) As List(Of String)
                Dim words As List(Of String) = Functs.TokenizeWords(text)
                Dim wordCounts As New Dictionary(Of String, Integer)()

                For Each word As String In words
                    If wordCounts.ContainsKey(word) Then
                        wordCounts(word) += 1
                    Else
                        wordCounts(word) = 1
                    End If
                Next

                ' Sort the words based on their counts in descending order
                Dim sortedWords As List(Of KeyValuePair(Of String, Integer)) = wordCounts.OrderByDescending(Function(x) x.Value).ToList()

                ' Get the top 10 words
                Dim topWords As List(Of String) = sortedWords.Take(10).Select(Function(x) x.Key).ToList()

                Return topWords
            End Function

            ''' <summary>
            ''' Returns a list of the unique words in the text
            ''' </summary>
            ''' <param name="text"></param>
            ''' <returns></returns>
            Public Shared Function GetUniqueWordsInText(ByRef text As String) As List(Of String)
                Dim words As List(Of String) = Functs.TokenizeWords(text)
                Dim uniqueWords As List(Of String) = words.Distinct().ToList()
                Return uniqueWords
            End Function

            Public Shared Sub PrintSentencesToConsole(ByRef iSentences As List(Of String))
                For Each sentence In iSentences
                    Console.WriteLine(sentence)
                Next
            End Sub

            ''' <summary>
            ''' Tokenizes the text into sentences based on punctuation end markers.
            ''' </summary>
            ''' <param name="text">The text to tokenize.</param>
            ''' <returns>A list of sentences.</returns>
            Public Shared Function TokenizeSentences(ByVal text As String) As List(Of Document.Sentence)
                Dim sentences As New List(Of Document.Sentence)()

                ' Split text into sentences based on punctuation end markers
                Dim pattern As String = $"(?<=[{String.Join("", SentenceEndMarkers)}])\s+"
                Dim sentenceTexts As String() = Regex.Split(text, pattern)

                For Each sentenceText As String In sentenceTexts
                    Dim isentence As New Document.Sentence()
                    isentence.OriginalSentence = sentenceText.Trim()

                    isentence.Clauses = Document.Sentence.GetClauses(text)
                    ' ... other sentence properties ...
                    sentences.Add(isentence)
                Next

                Return sentences
            End Function

            ''' <summary>
            ''' Tokenizes the sentence into words.
            ''' </summary>
            ''' <param name="sentenceText">The text of the sentence.</param>
            ''' <returns>A list of words.</returns>
            Public Shared Function TokenizeWords(ByVal sentenceText As String) As List(Of String)
                Dim words As New List(Of String)()

                ' Split sentence into words
                Dim wordPattern As String = "\b\w+\b"
                Dim wordMatches As MatchCollection = Regex.Matches(sentenceText, wordPattern)

                For Each match As Match In wordMatches
                    words.Add(match.Value.ToLower())
                Next

                Return words
            End Function
            Public Shared Function Top_N_Words(ByRef iDocContents As String, ByRef n As Integer) As List(Of String)
                Dim words As String() = iDocContents.Split(" ")
                Dim wordCount As New Dictionary(Of String, Integer)

                ' Count the frequency of each word in the corpus
                For Each word As String In words
                    If wordCount.ContainsKey(word) Then
                        wordCount(word) += 1
                    Else
                        wordCount.Add(word, 1)
                    End If
                Next

                ' Sort the dictionary by value (frequency) in descending order
                Dim sortedDict = (From entry In wordCount Order By entry.Value Descending Select entry).Take(n)
                Dim LSt As New List(Of String)
                ' Print the top ten words and their frequency
                For Each item In sortedDict
                    LSt.Add(item.Key)

                Next
                Return LSt
            End Function
        End Class
        ''' <summary>
        ''' Represents the vocabulary model for the corpus. 
        ''' (a record of words which can be looked up in the corpus)
        ''' It includes methods for adding new terms, calculating frequencies, TF-IDF, etc.
        ''' </summary>
        Public Class Vocabulary
            Public Current As List(Of VocabularyEntry)
            ''' <summary>
            ''' Used for TDM Calc
            ''' </summary>
            Private Docs As List(Of String)
            ''' <summary>
            ''' Prepare vocabulary for use
            ''' </summary>
            Public Sub New()
                Current = New List(Of VocabularyEntry)
                Docs = New List(Of String)
            End Sub
            ''' <summary>
            ''' Used to add Words or update a word in the vocabulary language model
            ''' </summary>
            ''' <param name="Term"></param>
            ''' <param name="Docs"></param>
            Public Sub AddNew(ByRef Term As String, ByRef Docs As List(Of String))
                Me.Docs = Docs
                Current.Add(New VocabularyEntry(Term,
                              CalcSequenceEncoding(Term),
                              CalcFrequency(Term),
                              CalcTF_IDF(Term)))

            End Sub

            Private Function CalcFrequency(ByRef Word As String) As Double
                ' Calculate frequency of term in the corpus (current)
                Dim count As Integer = 0
                For Each entry In Current
                    If entry.Text = Word Then

                        count += 1 + entry.Frequency
                    Else
                        Return 1
                    End If
                Next
                Return count
            End Function
            Public Function GetEntry(ByRef Token As String) As VocabularyEntry
                For Each item In Current
                    If item.Text = Token Then Return item
                Next
                Return Nothing
            End Function
            Public Function GetEntry(ByRef SequenceEmbedding As Integer) As VocabularyEntry
                For Each item In Current
                    If item.SequenceEncoding = SequenceEmbedding Then Return item
                Next
                Return Nothing
            End Function
            Public Function CheckEntry(ByRef Token As String) As Boolean
                For Each item In Current
                    If item.Text = Token Then Return True
                Next
                Return False
            End Function
            Private Function CalcInverseDocumentFrequency(ByRef Word As String, ByRef Docs As List(Of String)) As Double
                ' Calculate Inverse Document Frequency for the given term in the corpus
                Dim docsWithTerm As Integer = 0
                For Each doc In Docs
                    If doc.Contains(Word) Then
                        docsWithTerm += 1
                    End If
                Next
                Dim idf As Double = Math.Log(Docs.Count / (docsWithTerm + 1)) ' Adding 1 to avoid division by zero
                Return idf
            End Function

            Private Function CalcSequenceEncoding(ByRef Word As String) As Double
                ' Calculate sequence encoding based on the order of appearance in the corpus
                Dim encoding As Double = 0.0
                For Each entry In Current
                    If entry.Text = Word Then
                        encoding += 1
                    End If
                Next
                Return encoding
            End Function

            Private Function CalcTermFrequency(ByRef Word As String) As Double
                ' Calculate Term Frequency for the given term in the corpus
                Dim count As Integer = 0
                For Each entry In Current
                    If entry.Text = Word Then
                        count += 1
                    End If
                Next
                Return count
            End Function

            Private Function CalcTF_IDF(ByRef Word As String) As Double
                ' Calculate TF-IDF (Term Frequency-Inverse Document Frequency) for the given term in the corpus
                Dim tf As Double = CalcTermFrequency(Word)
                Dim idf As Double = CalcInverseDocumentFrequency(Word, Docs)
                Return tf * idf
            End Function
            ''' <summary>
            ''' Feature context is a way to add information with regards to the document,
            ''' Addind context elements such as features. 
            ''' Given a Sentiment (positive) , by marking the words in this document
            ''' present against the corpus vocabulary, it could be suggested that these would 
            ''' represent that topic in this document
            ''' </summary>
            Public Structure FeatureContext

                ''' <summary>
                ''' List of items Representing the context,
                ''' All entrys contained in the vocabulary are marked with a tag (present)(true)
                ''' if the are in the context else marked false
                ''' giving a one-shot encoding for the context this collection represents,
                ''' Ie:Sentiment/Topic etc
                ''' </summary>
                Public Present As List(Of VocabularyEntry)

                Public Type As String

                ''' <summary>
                ''' Encodes a Feature into the model,
                ''' Provide the label and the document words in the document
                ''' will be marked present in the context
                ''' Later these Oneshot encoding feature maybe used to increase the scoring vectors
                ''' Adding context to the document for a specific feature such as sentiment / Emotion / Topic.
                ''' Each topic should be encoded as a feature in the document
                '''
                ''' </summary>
                ''' <param name="CorpusVocab">Current Vocabulary </param>
                ''' <param name="iDocument"></param>
                ''' <param name="Label"></param>
                ''' <returns></returns>
                Public Shared Function GetDocumentContext(ByRef CorpusVocab As Vocabulary, ByRef iDocument As Document, ByRef Label As String) As Vocabulary.FeatureContext
                    Dim iContext As New Vocabulary.FeatureContext
                    Dim NewVocab As List(Of Vocabulary.VocabularyEntry) = CorpusVocab.Current

                    For Each item In NewVocab
                        For Each _item In iDocument.UniqueWords
                            If item.Text = _item Then
                                'Encode Presence in text
                                item.Present = True
                            End If
                        Next
                    Next
                    iContext.Present = NewVocab
                    iContext.Type = Label
                    Return iContext
                End Function

            End Structure





            Public Structure InputTextRecord
                Public Text As String
                Public Encoding As List(Of Integer)
                Public Inputblocks As List(Of List(Of Integer))
                Public Targetblocks As List(Of List(Of Integer))
                Public blocksize As Integer
                Public Shared Function GetBlocks(ByRef Embedding As List(Of Integer), ByRef Size As Integer, Optional Ofset As Integer = 0) As List(Of List(Of Integer))
                    Dim pos As Integer = 0
                    Dim newPos As Integer = Size
                    Dim blocks As New List(Of List(Of Integer))
                    Dim block As New List(Of Integer)
                    Do While pos < Embedding.Count - 1
                        For i = pos To newPos - 1
                            If Ofset > 0 Then
                                If i + Ofset < Embedding.Count - 1 Then

                                    block.Add(Embedding(i + Ofset))
                                    'block.Add(Embedding(i))
                                Else
                                    block.Add(Embedding(i))
                                End If
                            Else
                                block.Add(Embedding(i))
                            End If



                        Next
                        blocks.Add(block)
                        block = New List(Of Integer)
                        pos = newPos

                        If newPos + Size < Embedding.Count - 1 Then
                            newPos += Size
                        Else
                            newPos = Embedding.Count
                        End If

                    Loop

                    Return blocks
                End Function
                Public Shared Function GetTargetBlocks(ByRef Embedding As List(Of Integer), ByRef Size As Integer) As List(Of List(Of Integer))
                    Dim pos As Integer = 0
                    Dim newPos As Integer = Size
                    Dim blocks As New List(Of List(Of Integer))
                    Dim block As New List(Of Integer)
                    Do While pos < Embedding.Count - 1
                        For i = pos To newPos - 1
                            block.Add(Embedding(i))

                        Next
                        blocks.Add(block)
                        block = New List(Of Integer)
                        pos = newPos
                        If newPos + Size < Embedding.Count - 1 Then
                            newPos += Size
                        Else
                            newPos = Embedding.Count
                        End If

                    Loop

                    Return blocks
                End Function
            End Structure
            Public Class Encode
                Public Shared Function Encode_Text(ByRef Text As String, ByRef Vocab As List(Of VocabularyEntry), ByRef Type As VocabularyType) As List(Of Integer)
                    Dim iOutput As New List(Of Integer)
                    Select Case Type
                        Case VocabularyType.Character
                            Dim Chars = Tokenizer.TokenizeByCharacter(Text)

                            For Each item In Chars
                                If CheckVocabulary(item.Value.ToLower, Vocab) = True Then
                                    iOutput.Add(Decode.DecodeText(item.Value.ToLower, Vocab))
                                End If
                            Next
                        Case VocabularyType.Word
                            Dim Words = Tokenizer.TokenizeByWord(Text)

                            For Each item In Words
                                If CheckVocabulary(item.Value.ToLower, Vocab) = True Then
                                    iOutput.Add(Decode.DecodeText(item.Value.ToLower, Vocab))
                                End If
                            Next
                        Case VocabularyType.Sentence
                            Dim Sents = Tokenizer.TokenizeBySentence(Text)

                            For Each item In Sents
                                If CheckVocabulary(item.Value, Vocab) = True Then
                                    iOutput.Add(Decode.DecodeText(item.Value.ToLower, Vocab))
                                End If
                            Next
                    End Select
                    Return iOutput
                End Function
                Public Shared Function EncodeChars(VocabList As List(Of String)) As List(Of VocabularyEntry)
                    Dim vocabulary As New List(Of VocabularyEntry)
                    Dim EncodingValue As Integer = 1
                    For Each item In VocabList
                        Dim newVocabRecord As New VocabularyEntry
                        newVocabRecord.Encoding = EncodingValue
                        newVocabRecord.Text = item
                        EncodingValue += 1
                        vocabulary.Add(newVocabRecord)
                    Next
                    Return vocabulary
                End Function
                Public Shared Function EncodeWords(VocabList As List(Of String)) As List(Of VocabularyEntry)
                    Dim vocabulary As New List(Of VocabularyEntry)
                    Dim EncodingValue As Integer = 1
                    For Each item In VocabList
                        Dim newVocabRecord As New VocabularyEntry
                        newVocabRecord.Encoding = EncodingValue
                        newVocabRecord.Text = item
                        EncodingValue += 1
                        vocabulary.Add(newVocabRecord)
                    Next
                    Return vocabulary
                End Function
                Public Shared Function AddNewEncoding(ByRef Word As String, ByRef Vocab As List(Of VocabularyEntry)) As List(Of VocabularyEntry)
                    Dim NewVocab As New List(Of VocabularyEntry)
                    If CheckVocabulary(Word, Vocab) = False Then
                        NewVocab = Vocab
                        Dim NewItem As New VocabularyEntry
                        NewItem.Text = Word
                        NewItem.Encoding = Vocab.Count
                        NewVocab.Add(NewItem)
                        Return NewVocab
                    Else
                        Return Vocab
                    End If
                End Function
                Public Shared Function CheckVocabulary(ByRef Word As String, ByRef Vocab As List(Of VocabularyEntry)) As Boolean

                    For Each item In Vocab
                        If item.Text = Word Then
                            Return True
                        End If
                    Next
                    Return False
                End Function
            End Class
            Public Class Decode
                Public Shared Function DecodeInteger(ByRef Lookup As Integer, ByRef Vocabulary As List(Of VocabularyEntry))
                    For Each item In Vocabulary
                        If item.Encoding = Lookup Then
                            Return item.Text
                        End If
                    Next
                    Return "Not found in vocabulary"
                End Function
                Public Shared Function DecodeText(ByRef Lookup As String, ByRef Vocabulary As List(Of VocabularyEntry))
                    For Each item In Vocabulary
                        If item.Text = Lookup Then
                            Return item.Encoding
                        End If
                    Next
                    Return "Not found in vocabulary"
                End Function
            End Class
            Public Class VocabularyEntry
                Public Text As String
                Public Encoding As Integer
                Public Frequency As Integer
                Public Present As Boolean
                Public SequenceEncoding As Integer
                Public TF_IDF As Double
                Public Sub New()

                End Sub
                Public Sub New(text As String, sequenceEncoding As Integer, frequency As Integer, tF_IDF As Double)
                    If text Is Nothing Then
                        Throw New ArgumentNullException(NameOf(text))
                    End If

                    Me.Text = text
                    Me.SequenceEncoding = sequenceEncoding
                    Me.Frequency = frequency
                    Me.TF_IDF = tF_IDF
                End Sub

            End Class
            Public Enum VocabularyType
                Character
                Word
                Sentence
            End Enum
            Private Shared Function CreateCharVocabulary(ByRef text As String) As List(Of VocabularyEntry)

                Dim RecordList = CreateUniqueChars(text)

                Dim vocabulary As List(Of VocabularyEntry) = Encode.EncodeChars(RecordList)
                Return vocabulary
            End Function
            Private Shared Function CreateWordVocabulary(ByRef text As String) As List(Of VocabularyEntry)

                Dim RecordList = CreateUniqueWords(text)

                Dim vocabulary As List(Of VocabularyEntry) = Encode.EncodeWords(RecordList)
                Return vocabulary
            End Function
            Private Shared Function CreateSentenceVocabulary(ByRef text As String) As List(Of VocabularyEntry)

                Dim RecordList = CreateUniqueSentences(text)

                Dim vocabulary As List(Of VocabularyEntry) = Encode.EncodeWords(RecordList)
                Return vocabulary
            End Function
            Public Shared Function UpdateVocabulary(ByRef Text As String, ByRef vocab As List(Of VocabularyEntry))
                Return Encode.AddNewEncoding(Text, vocab)
            End Function

            Public Shared Function CreateUniqueSentences(ByRef Text As String) As List(Of String)
                Dim Words = Tokenizer.TokenizeBySentence(Text)
                Dim WordList As New List(Of String)
                For Each item In Words
                    If WordList.Contains(item.Value.ToLower) = False Then
                        WordList.Add(item.Value.ToLower)
                    End If

                Next

                Return WordList
            End Function

            Public Shared Function CreateUniqueWords(ByRef Text As String) As List(Of String)
                Dim Words = Tokenizer.TokenizeByWord(Text)
                Dim WordList As New List(Of String)
                For Each item In Words
                    If WordList.Contains(item.Value.ToLower) = False Then
                        WordList.Add(item.Value.ToLower)
                    End If

                Next

                Return WordList
            End Function

            Public Shared Function CreateUniqueChars(ByRef Text As String) As List(Of String)
                Dim Chars = Tokenizer.TokenizeByCharacter(Text)
                Dim CharList As New List(Of String)
                For Each item In Chars
                    If CharList.Contains(item.Value.ToLower) = False Then
                        CharList.Add(item.Value.ToLower)
                    End If

                Next

                Return CharList
            End Function
            Public Shared Function CreateVocabulary(ByRef Text As String, vType As VocabularyType) As List(Of VocabularyEntry)
                Select Case vType
                    Case VocabularyType.Character
                        Return CreateCharVocabulary(Text)
                    Case VocabularyType.Word
                        Return CreateWordVocabulary(Text)
                    Case VocabularyType.Sentence
                        Return CreateSentenceVocabulary(Text)
                End Select
                Return New List(Of VocabularyEntry)
            End Function
        End Class
        '    Positional Encoding : 
        '    To provide positional information to the model, positional encodings.
        '    These encodings are added to the input embeddings to capture the order of the tokens in the sequence.
        '    Positional Encoding : 
        '    To provide positional information to the model, positional encodings.
        '    These encodings are added to the input embeddings to capture the order of the tokens in the sequence.
        Public Class PositionalEncoding
            Private ReadOnly encodingMatrix As List(Of List(Of Double))
            Private InternalVocab As Corpus.Vocabulary
            Public Sub New(maxLength As Integer, embeddingSize As Integer, ByRef Vocab As Corpus.Vocabulary)
                InternalVocab = Vocab
                encodingMatrix = New List(Of List(Of Double))()
                ' Create the encoding matrix
                For pos As Integer = 0 To maxLength - 1
                    Dim encodingRow As List(Of Double) = New List(Of Double)()
                    For i As Integer = 0 To embeddingSize - 1
                        Dim angle As Double = pos / Math.Pow(10000, (2 * i) / embeddingSize)
                        encodingRow.Add(Math.Sin(angle))
                        encodingRow.Add(Math.Cos(angle))
                    Next
                    encodingMatrix.Add(encodingRow)
                Next
            End Sub
            Public Function Encode(inputTokens As List(Of String)) As List(Of List(Of Double))
                Dim encodedInputs As List(Of List(Of Double)) = New List(Of List(Of Double))()

                For Each token As String In inputTokens
                    Dim tokenEncoding As List(Of Double) = New List(Of Double)()

                    ' Find the index of the token in the vocabulary
                    ' For simplicity, let's assume a fixed vocabulary
                    Dim tokenIndex As Integer = GetTokenIndex(token)

                    ' Retrieve the positional encoding for the token
                    If tokenIndex >= 0 Then
                        tokenEncoding = encodingMatrix(tokenIndex)
                    Else
                        ' Handle unknown tokens if necessary
                    End If

                    encodedInputs.Add(tokenEncoding)
                Next

                Return encodedInputs
            End Function
            Private Function GetTokenIndex(token As String) As Integer
                ' Retrieve the index of the token in the vocabulary
                ' For simplicity, let's assume a fixed vocabulary
                Dim vocabulary As List(Of String) = GetVocabulary(InternalVocab)
                Return vocabulary.IndexOf(token)
            End Function
            Private Function GetVocabulary(ByRef Vocab As Corpus.Vocabulary) As List(Of String)
                ' Return the vocabulary list
                ' Modify this function as per your specific vocabulary
                Dim Lst As New List(Of String)
                For Each item In Vocab.Current
                    Lst.Add(item.Text)
                Next
                Return Lst
            End Function
        End Class
        Public Class PositionalDecoder
            Private ReadOnly decodingMatrix As List(Of List(Of Double))
            Private InternalVocab As Corpus.Vocabulary
            Public Sub New(maxLength As Integer, embeddingSize As Integer, ByRef Vocab As Corpus.Vocabulary)
                decodingMatrix = New List(Of List(Of Double))()
                InternalVocab = Vocab
                ' Create the decoding matrix
                For pos As Integer = 0 To maxLength - 1
                    Dim decodingRow As List(Of Double) = New List(Of Double)()

                    For i As Integer = 0 To embeddingSize - 1
                        Dim angle As Double = pos / Math.Pow(10000, (2 * i) / embeddingSize)
                        decodingRow.Add(Math.Sin(angle))
                        decodingRow.Add(Math.Cos(angle))
                    Next

                    decodingMatrix.Add(decodingRow)
                Next
            End Sub

            Public Function Decode(encodedInputs As List(Of List(Of Double))) As List(Of String)
                Dim decodedTokens As List(Of String) = New List(Of String)()

                For Each encoding As List(Of Double) In encodedInputs
                    ' Retrieve the token index based on the encoding
                    Dim tokenIndex As Integer = GetTokenIndex(encoding)

                    ' Retrieve the token based on the index
                    If tokenIndex >= 0 Then
                        Dim token As String = GetToken(tokenIndex)
                        decodedTokens.Add(token)
                    Else
                        ' Handle unknown encodings if necessary
                    End If
                Next

                Return decodedTokens
            End Function

            Private Function GetTokenIndex(encoding As List(Of Double)) As Integer
                ' Retrieve the index of the token based on the encoding
                ' For simplicity, let's assume a fixed vocabulary
                Dim vocabulary As List(Of String) = GetVocabulary(InternalVocab)

                For i As Integer = 0 To decodingMatrix.Count - 1
                    If encoding.SequenceEqual(decodingMatrix(i)) Then
                        Return i
                    End If
                Next

                Return -1 ' Token not found
            End Function

            Private Function GetToken(tokenIndex As Integer) As String
                ' Retrieve the token based on the index
                ' For simplicity, let's assume a fixed vocabulary
                Dim vocabulary As List(Of String) = GetVocabulary(InternalVocab)

                If tokenIndex >= 0 AndAlso tokenIndex < vocabulary.Count Then
                    Return vocabulary(tokenIndex)
                Else
                    Return "Unknown" ' Unknown token
                End If
            End Function

            Private Function GetVocabulary(ByRef Vocab As Corpus.Vocabulary) As List(Of String)
                ' Return the vocabulary list
                ' Modify this function as per your specific vocabulary
                Dim Lst As New List(Of String)
                For Each item In Vocab.Current
                    Lst.Add(item.Text)
                Next
                Return Lst
            End Function
        End Class
    End Class
    Public Class TrainingTextGenerator
        Public EntityTypes As List(Of String)
        Public EntityList As List(Of Entity)

        Private Shared Random As New Random()

        Public Subjects As String()
        Public Verbs As String()
        Public Objects As String()
        Function GetCVS() As List(Of String)
            Dim lst As New List(Of String)
            lst.Add(DataScientistCV)
            lst.Add(WebDeveloperCV)
            lst.Add(SoftwareDeveloperCV)

            Return lst
        End Function
        Function DataScientistCV() As String
            Return "
Name: Jane Smith
Profession: Data Scientist
Experience: 8 years
Skills: Python, R, Machine Learning
Qualifications: Ph.D. in Data Science
"

        End Function

        ' CV Examples
        Function SoftwareDeveloperCV()
            Return "
Name: John Doe
Profession: Software Developer
Experience: 5 years
Skills: Python, Java
Qualifications: Bachelor's degree in Computer Science
"

        End Function

        Function WebDeveloperCV()
            Return "
Name: Mark Johnson
Profession: Web Developer
Experience: 10 years
Skills: HTML, CSS, JavaScript, PHP
Qualifications: Bachelor's degree in Web Development
"

        End Function
        Public Sub New(entityTypes As List(Of String),
                       entityList As List(Of Entity),
                       subjects() As String,
                       verbs() As String,
                       objects() As String)
            If entityTypes Is Nothing Then
                Throw New ArgumentNullException(NameOf(entityTypes))
            End If

            If entityList Is Nothing Then
                Throw New ArgumentNullException(NameOf(entityList))
            End If

            If subjects Is Nothing Then
                Throw New ArgumentNullException(NameOf(subjects))
            End If

            If verbs Is Nothing Then
                Throw New ArgumentNullException(NameOf(verbs))
            End If

            If objects Is Nothing Then
                Throw New ArgumentNullException(NameOf(objects))
            End If

            Me.EntityTypes = entityTypes
            Me.EntityList = entityList
            Me.Subjects = subjects
            Me.Verbs = verbs
            Me.Objects = objects
        End Sub
        Public Shared Function GetAnotatedTopicList() As List(Of String)
            Dim annotatedData As New List(Of String)
            annotatedData.Add("Gravity: concept")
            annotatedData.Add("Force: concept")
            annotatedData.Add("Energy: concept")
            annotatedData.Add("Electricity: concept")
            annotatedData.Add("Magnetism: concept")
            annotatedData.Add("Atoms: concept")
            annotatedData.Add("Light: concept")
            annotatedData.Add("Sound: concept")
            annotatedData.Add("Friction: concept")
            annotatedData.Add("Heat: concept")
            annotatedData.Add("Acceleration: concept")
            annotatedData.Add("Velocity: concept")
            annotatedData.Add("Mass: concept")
            annotatedData.Add("Density: concept")
            annotatedData.Add("Volume: concept")
            annotatedData.Add("Pressure: concept")
            annotatedData.Add("Temperature: concept")
            annotatedData.Add("Electric circuit: concept")
            annotatedData.Add("Solar system: concept")
            annotatedData.Add("Photosynthesis: concept")
            annotatedData.Add("Chemical reactions: concept")
            annotatedData.Add("Simple machines: concept")
            annotatedData.Add("Ecosystem: concept")
            annotatedData.Add("States of matter: concept")
            annotatedData.Add("Genetics: concept")
            annotatedData.Add("Plant growth: concept")
            annotatedData.Add("Human body: concept")
            annotatedData.Add("Pollution: concept")
            annotatedData.Add("Water cycle: concept")
            annotatedData.Add("Weather: concept")
            annotatedData.Add("Electric motors: concept")
            annotatedData.Add("Renewable energy: concept")
            annotatedData.Add("Food chain: concept")
            annotatedData.Add("Space exploration: concept")
            annotatedData.Add("Computer programming: concept")
            annotatedData.Add("Scientific method: concept")
            annotatedData.Add("Photosynthesis: concept")
            annotatedData.Add("Periodic table: concept")
            annotatedData.Add("DNA: concept")
            annotatedData.Add("Motion: concept")
            annotatedData.Add("Newton's laws: concept")
            annotatedData.Add("Gravity: concept")
            annotatedData.Add("Sound waves: concept")
            annotatedData.Add("Electricity: concept")
            annotatedData.Add("Atoms: concept")
            annotatedData.Add("Light: concept")
            annotatedData.Add("Matter: concept")
            annotatedData.Add("Solar energy: concept")
            annotatedData.Add("Chemical reactions: concept")
            annotatedData.Add("Genetics: concept")
            annotatedData.Add("Plant growth: concept")
            annotatedData.Add("Human body: concept")
            ' Biology Entities
            annotatedData.Add("Human: organism")
            annotatedData.Add("Dog: organism")
            annotatedData.Add("Tree: organism")
            annotatedData.Add("Bird: organism")
            annotatedData.Add("Insect: organism")
            annotatedData.Add("Fish: organism")
            annotatedData.Add("Amoeba: organism")
            annotatedData.Add("Bacteria: organism")
            annotatedData.Add("Flower: organism")
            annotatedData.Add("Fruit: organism")
            annotatedData.Add("Photosynthesis: process")
            annotatedData.Add("Cell: part")
            annotatedData.Add("Digestive system: system")
            annotatedData.Add("DNA: molecule")
            annotatedData.Add("Respiratory system: system")
            annotatedData.Add("Life cycle: cycle")
            annotatedData.Add("Circulatory system: system")
            annotatedData.Add("Ecosystem: system")
            annotatedData.Add("Classification: classification")
            annotatedData.Add("Nervous system: system")

            ' Computer Science Entities
            annotatedData.Add("Programming: field")
            annotatedData.Add("Computer: device")
            annotatedData.Add("Algorithm: concept")
            annotatedData.Add("Coding: skill")
            annotatedData.Add("Network: concept")
            annotatedData.Add("Cybersecurity: field")
            annotatedData.Add("Graphics: concept")
            annotatedData.Add("Artificial intelligence: concept")
            annotatedData.Add("Software: component")
            annotatedData.Add("Game: application")
            annotatedData.Add("Internet: network")
            annotatedData.Add("Hardware: component")
            annotatedData.Add("Data encryption: technique")
            annotatedData.Add("Memory: component")
            annotatedData.Add("Cloud computing: concept")
            annotatedData.Add("Robot: application")

            ' NLP Entities
            annotatedData.Add("Natural language processing: field")
            annotatedData.Add("Chatbot: application")
            annotatedData.Add("Voice recognition: technology")
            annotatedData.Add("Sentiment analysis: technique")
            annotatedData.Add("Machine translation: technique")
            annotatedData.Add("Spam filter: application")
            annotatedData.Add("Named entity recognition: technique")
            annotatedData.Add("Text summarization: technique")
            annotatedData.Add("Text classification: technique")
            annotatedData.Add("Topic modeling: technique")
            annotatedData.Add("Search engine: application")
            annotatedData.Add("Part-of-speech tagging: technique")
            annotatedData.Add("Text generation: technique")
            annotatedData.Add("Information extraction: technique")
            annotatedData.Add("Language model: model")

            annotatedData.Add("John Doe: person")
            annotatedData.Add("Emma Smith: person")
            annotatedData.Add("Microsoft: organization")
            annotatedData.Add("Apple Inc.: organization")
            annotatedData.Add("New York City: location")
            annotatedData.Add("London: location")
            annotatedData.Add("Tom Johnson: person")
            annotatedData.Add("Google: organization")
            annotatedData.Add("Paris: location")
            annotatedData.Add("Emily Brown: person")
            annotatedData.Add("Amazon: organization")
            annotatedData.Add("Los Angeles: location")
            annotatedData.Add("Mark Thompson: person")
            annotatedData.Add("Facebook: organization")
            annotatedData.Add("San Francisco: location")
            annotatedData.Add("Robert Anderson: person")
            annotatedData.Add("Twitter: organization")
            annotatedData.Add("Chicago: location")
            annotatedData.Add("Sarah Davis: person")
            annotatedData.Add("IBM: organization")
            annotatedData.Add("Seattle: location")
            annotatedData.Add("James Wilson: person")
            annotatedData.Add("Oracle: organization")
            annotatedData.Add("Berlin: location")
            annotatedData.Add("Jennifer Lee: person")
            annotatedData.Add("Intel: organization")
            annotatedData.Add("Tokyo: location")
            ' Biology Entities
            annotatedData.Add("Human: organism")
            annotatedData.Add("Dog: organism")
            annotatedData.Add("Tree: organism")
            annotatedData.Add("Bird: organism")
            annotatedData.Add("Insect: organism")
            annotatedData.Add("Fish: organism")
            annotatedData.Add("Amoeba: organism")
            annotatedData.Add("Bacteria: organism")
            annotatedData.Add("Flower: organism")
            annotatedData.Add("Fruit: organism")
            annotatedData.Add("Photosynthesis: process")
            annotatedData.Add("Cell: part")
            annotatedData.Add("Digestive system: system")
            annotatedData.Add("DNA: molecule")
            annotatedData.Add("Respiratory system: system")
            annotatedData.Add("Life cycle: cycle")
            annotatedData.Add("Circulatory system: system")
            annotatedData.Add("Ecosystem: system")
            annotatedData.Add("Classification: classification")
            annotatedData.Add("Nervous system: system")

            ' Computer Science Entities
            annotatedData.Add("Programming: field")
            annotatedData.Add("Computer: device")
            annotatedData.Add("Algorithm: concept")
            annotatedData.Add("Coding: skill")
            annotatedData.Add("Network: concept")
            annotatedData.Add("Cybersecurity: field")
            annotatedData.Add("Graphics: concept")
            annotatedData.Add("Artificial intelligence: concept")
            annotatedData.Add("Software: component")
            annotatedData.Add("Game: application")
            annotatedData.Add("Internet: network")
            annotatedData.Add("Hardware: component")
            annotatedData.Add("Data encryption: technique")
            annotatedData.Add("Memory: component")
            annotatedData.Add("Cloud computing: concept")
            annotatedData.Add("Robot: application")

            ' NLP Entities
            annotatedData.Add("Natural language processing: field")
            annotatedData.Add("Chatbot: application")
            annotatedData.Add("Voice recognition: technology")
            annotatedData.Add("Sentiment analysis: technique")
            annotatedData.Add("Machine translation: technique")
            annotatedData.Add("Spam filter: application")
            annotatedData.Add("Named entity recognition: technique")
            annotatedData.Add("Text summarization: technique")
            annotatedData.Add("Text classification: technique")
            annotatedData.Add("Topic modeling: technique")
            annotatedData.Add("Search engine: application")
            annotatedData.Add("Part-of-speech tagging: technique")
            annotatedData.Add("Text generation: technique")
            annotatedData.Add("Information extraction: technique")
            annotatedData.Add("Language model: model")

            Return annotatedData
        End Function

        ''' <summary>
        ''' allows for adding quick training data lists
        ''' </summary>
        ''' <param name="annotatedData"></param>
        ''' <returns></returns>
        Public Function AddAnnotatedData(annotatedData As List(Of String)) As List(Of Entity)
            Dim Entitys As New List(Of Entity)
            For Each entry In annotatedData
                Dim parts As String() = entry.Split(":")
                Dim text As String = parts(0).Trim()
                Dim entity As String = parts(1).Trim()
                Dim NewEnt As New Entity

                For Each item In entity
                    NewEnt.Type = text
                    NewEnt.Value = item
                    Entitys.Add(NewEnt)
                Next

            Next
            Return Entitys
        End Function

        Public Function GenerateSentence() As String
            Dim subject As String = GetRandomItem(Subjects)
            Dim verb As String = GetRandomItem(Verbs)
            Dim iobject As String = GetRandomItem(Objects)

            Dim sentence As String = $"{subject} {verb} {iobject}."

            ' Add contextual variation
            Dim context As String = GetRandomContext()
            sentence = $"{context} {sentence}"

            Return sentence
        End Function

        Public Function GenerateNamedEntity() As String
            Dim entityType As String = GetRandomEntity()
            Dim entityName As String = String.Empty
            Dim Words As New List(Of String)
            For Each item In EntityList
                If item.Type = entityType Then
                    Words.Add(item.Value)
                End If
            Next


            entityName = GetRandomItem(Words.ToArray)




            Return entityName
        End Function

        Private Shared Function GetRandomItem(items As String()) As String
            Dim index As Integer = Random.Next(0, items.Length)
            Return items(index)
        End Function
        ''' <summary>
        ''' Returns a list of various code snippets
        ''' </summary>
        ''' <returns></returns>
        Public Function LoadSourceCodeTrainingData() As List(Of String)
            ' Training data
            Dim trainingData As List(Of String) = New List(Of String) From {
                    "public void method1() {",
                    "    Console.WriteLine(""Hello, world!"");",
                    "}",
                    "",
                    "public int method2(int x, int y) {",
                    "    int sum = x + y;",
                    "    return sum;",
                    "}"
                }

            trainingData.Add("Sub HelloWorld()")
            trainingData.Add("    Console.WriteLine(""Hello, World!"")")
            trainingData.Add("End Sub")

            Console.ReadLine()
            trainingData.Add("Public Sub CalculateSum(a As Integer, b As Integer) As Integer")
            trainingData.Add("    Return a + b")
            trainingData.Add("End Sub")

            trainingData.Add("Public Function GetMaxValue(numbers As List(Of Integer)) As Integer")
            trainingData.Add("    Return numbers.Max()")
            trainingData.Add("End Function")

            trainingData.Add("Public Sub PrintMessage(message As String)")
            trainingData.Add("    Console.WriteLine(message)")
            trainingData.Add("End Sub")

            trainingData.Add("Public Function IsPrime(number As Integer) As Boolean")
            trainingData.Add("    If number < 2 Then")
            trainingData.Add("        Return False")
            trainingData.Add("    End If")
            trainingData.Add("    For i As Integer = 2 To Math.Sqrt(number)")
            trainingData.Add("        If number Mod i = 0 Then")
            trainingData.Add("            Return False")
            trainingData.Add("        End If")
            trainingData.Add("    Next")
            trainingData.Add("    Return True")
            trainingData.Add("End Function")

            trainingData.Add("Public Sub SortArray(numbers As Integer())")
            trainingData.Add("    Array.Sort(numbers)")
            trainingData.Add("End Sub")

            trainingData.Add("Public Function ConvertToUpperCase(text As String) As String")
            trainingData.Add("    Return text.ToUpper()")
            trainingData.Add("End Function")

            trainingData.Add("Public Sub WaitForInput()")
            trainingData.Add("    Console.ReadLine()")
            trainingData.Add("End Sub")

            trainingData.Add("Public Function ConcatenateStrings(strings As String()) As String")
            trainingData.Add("    Return String.Concat(strings)")
            trainingData.Add("End Function")

            trainingData.Add("Public Sub PerformTask(task As Action)")
            trainingData.Add("    task.Invoke()")
            trainingData.Add("End Sub")
            'Dim trainingData As List(Of String) = New List(Of String)()
            trainingData.Add("Public Class MyClass")
            trainingData.Add("    Public Sub MyMethod()")
            trainingData.Add("        Console.WriteLine(""Hello, world!"")")
            trainingData.Add("    End Sub")
            trainingData.Add("End Class")

            trainingData.Add("Public Class MyClass")
            trainingData.Add("    Public Sub MyMethod()")
            trainingData.Add("        Console.WriteLine(""Hello, world!"")")
            trainingData.Add("    End Sub")
            trainingData.Add("End Class")

            trainingData.Add("Public Class Rectangle")
            trainingData.Add("    Private width As Integer")
            trainingData.Add("    Private height As Integer")
            trainingData.Add("    Public Sub SetDimensions(w As Integer, h As Integer)")
            trainingData.Add("        width = w")
            trainingData.Add("        height = h")
            trainingData.Add("    End Sub")
            trainingData.Add("    Public Function GetArea() As Integer")
            trainingData.Add("        Return width * height")
            trainingData.Add("    End Function")
            trainingData.Add("End Class")
            trainingData.Add("Public Class Singleton")
            trainingData.Add("    Private Shared instance As Singleton")
            trainingData.Add("    Private Sub New()")
            trainingData.Add("    End Sub")
            trainingData.Add("    Public Shared Function GetInstance() As Singleton")
            trainingData.Add("        If instance Is Nothing Then")
            trainingData.Add("            instance = New Singleton()")
            trainingData.Add("        End If")
            trainingData.Add("        Return instance")
            trainingData.Add("    End Function")
            trainingData.Add("End Class")

            trainingData.Add("Public Interface ISubject")
            trainingData.Add("    Sub Attach(observer As IObserver)")
            trainingData.Add("    Sub Detach(observer As IObserver)")
            trainingData.Add("    Sub Notify()")
            trainingData.Add("End Interface")

            trainingData.Add("Public Class ConcreteSubject Implements ISubject")
            trainingData.Add("    Private observers As List(Of IObserver) = New List(Of IObserver)()")
            trainingData.Add("    Public Sub Attach(observer As IObserver) Implements ISubject.Attach")
            trainingData.Add("        observers.Add(observer)")
            trainingData.Add("    End Sub")
            trainingData.Add("    Public Sub Detach(observer As IObserver) Implements ISubject.Detach")
            trainingData.Add("        observers.Remove(observer)")
            trainingData.Add("    End Sub")
            trainingData.Add("    Public Sub Notify() Implements ISubject.Notify")
            trainingData.Add("        For Each observer As IObserver In observers")
            trainingData.Add("            observer.Update()")
            trainingData.Add("        Next")
            trainingData.Add("    End Sub")
            trainingData.Add("End Class")

            trainingData.Add("Public Interface IObserver")
            trainingData.Add("    Sub Update()")
            trainingData.Add("End Interface")

            trainingData.Add("Public Class ConcreteObserver Implements IObserver")
            trainingData.Add("    Public Sub Update() Implements IObserver.Update")
            trainingData.Add("        ' Perform observer update logic here")
            trainingData.Add("    End Sub")
            trainingData.Add("End Class")

            trainingData.Add("Public Sub Main()")
            trainingData.Add("    Dim rect As New Rectangle()")
            trainingData.Add("    rect.SetDimensions(5, 10)")
            trainingData.Add("    Dim area As Integer = rect.GetArea()")
            trainingData.Add("    Console.WriteLine(""Area: "" & area)")
            trainingData.Add("End Sub")
            Return trainingData
        End Function
        Public entityRegExTypes As New Dictionary(Of String, String)() From {
            {"PERSON", "([A-Z][a-zA-Z]+)"},
            {"ORGANIZATION", "([A-Z][\w&]+(?:\s[A-Z][\w&]+)*)"},
            {"LOCATION", "([A-Z][\w\s&-]+)"},
            {"DATE", "(\d{1,2}\/\d{1,2}\/\d{2,4})"},
            {"TIME", "(\d{1,2}:\d{2}(?::\d{2})?)"},
            {"MONEY", "(\$\d+(?:\.\d{2})?)"},
            {"PERCENT", "(\d+(?:\.\d+)?%)"}
           }
        Private Function GetRandomContext() As String
            Dim entity As String = GenerateNamedEntity()
            Dim contextType As String = GetRandomItem(New String() {"before", "after"})

            Select Case contextType
                Case "before"
                    Return $"In the context of {entity},"
                Case "after"
                    Return $"Considering {entity},"
                Case Else
                    Return String.Empty
            End Select
        End Function

        Private Function GetRandomEntity() As String

            Dim index As Integer = Random.Next(0, EntityTypes.Count)
            Return EntityTypes(index)
        End Function
        Public Function GetEntitySentencePatterns() As List(Of String)
            ' Add question patterns to the list
            Dim patterns As New List(Of String)()
            patterns.Add("What is the formula for calculating [attribute]?")
            patterns.Add("How does [entity] work?")
            patterns.Add("What are the basic principles of [entity]?")
            patterns.Add("What is the difference between [entity1] and [entity2]?")
            patterns.Add("How does [entity] affect our daily life?")
            patterns.Add("Why is [entity] important in [subject]?")
            patterns.Add("What are the real-world applications of [entity]?")
            patterns.Add("How do scientists study [entity]?")
            patterns.Add("What are some interesting facts about [entity]?")
            patterns.Add("Can you explain [concept] in simple terms?")
            patterns.Add("What are the different types of [entity]?")
            patterns.Add("How does [entity] help us understand [concept]?")
            patterns.Add("Why is [concept] important in [subject]?")
            patterns.Add("What are the steps involved in [process]?")
            patterns.Add("Can you provide examples of [entity] in everyday life?")
            patterns.Add("What are the main components of [entity]?")
            patterns.Add("How does [entity] relate to [subject]?")
            patterns.Add("What happens when [action] [entity]?")
            patterns.Add("What are the advantages and disadvantages of [entity]?")
            patterns.Add("How can I perform [action] with [entity]?")
            patterns.Add("What are the key features of [entity]?")
            patterns.Add("Can you explain the concept of [entity] using an analogy?")
            patterns.Add("What are the safety precautions when dealing with [entity]?")
            patterns.Add("How does [entity] compare to [similar entity]?")
            patterns.Add("What are the properties of [entity]?")
            patterns.Add("How does [entity] interact with [other entity]?")
            patterns.Add("Why is it important to study [subject]?")
            patterns.Add("What are the limitations of [entity]?")
            patterns.Add("What are the practical uses of [concept]?")
            patterns.Add("How does [entity] impact the environment?")
            patterns.Add("Can you explain the concept of [entity] through an experiment?")
            patterns.Add("What are the future advancements in [subject]?")
            patterns.Add("How does [entity] contribute to sustainable development?")
            patterns.Add("What are the ethical considerations related to [entity]?")
            patterns.Add("How can we solve [problem] using [entity]?")
            patterns.Add("What are the challenges faced in studying [subject]?")
            patterns.Add("How does [entity] support scientific research?")
            patterns.Add("What are the key discoveries or inventions related to [subject]?")
            patterns.Add("How can we use [entity] to improve our daily lives?")
            patterns.Add("What are the different branches or fields within [subject]?")
            patterns.Add("What are the common misconceptions about [entity]?")
            patterns.Add("How does [entity] impact the economy?")
            patterns.Add("Can you provide a step-by-step explanation of [process]?")
            patterns.Add("What are the implications of [entity] on society?")
            patterns.Add("How does [entity] contribute to technological advancements?")
            patterns.Add("What are the current research areas in [subject]?")
            patterns.Add("How does [entity] help us understand the universe?")
            patterns.Add("What are the key principles of [concept]?")
            patterns.Add("Can you explain [entity] using visual aids?")
            patterns.Add("What are the career opportunities in [subject]?")
            patterns.Add("When did [entity] [action]?")
            patterns.Add("What is the [attribute] of [entity]?")
            patterns.Add("How does [entity] [action]?")
            patterns.Add("Who is [entity]'s [relation]?")
            patterns.Add("Where does [entity] [action]?")
            patterns.Add("Why did [entity] [action]?")
            patterns.Add("How many [entity]s are there?")
            patterns.Add("Which [entity] [action]?")
            patterns.Add("Tell me about [entity].")
            patterns.Add("What are the benefits of [entity]?")
            patterns.Add("Can you explain [entity] in simple terms?")
            patterns.Add("What are the different types of [entity]?")
            patterns.Add("Is [entity] a reliable source?")
            patterns.Add("How can I [action] [entity]?")
            patterns.Add("What are the main features of [entity]?")
            patterns.Add("Can you provide examples of [entity]?")
            patterns.Add("What are the steps to [action] [entity]?")
            patterns.Add("What is the purpose of [entity]?")
            patterns.Add("Are there any alternatives to [entity]?")
            patterns.Add("What are the risks associated with [entity]?")
            patterns.Add("How can I contact [entity]?")
            patterns.Add("Is [entity] available in my country?")
            patterns.Add("What are the system requirements for [entity]?")
            patterns.Add("What is the average cost of [entity]?")
            patterns.Add("Can you recommend any resources for learning about [entity]?")
            patterns.Add("What are the legal implications of [action] [entity]?")
            patterns.Add("How does [entity] compare to similar products?")
            patterns.Add("What are the key components of [entity]?")
            patterns.Add("What are the security measures in place for [entity]?")
            patterns.Add("Can you explain the working principle of [entity]?")
            patterns.Add("How long does it take to [action] [entity]?")
            patterns.Add("What are the licensing options for [entity]?")
            patterns.Add("Is [entity] compatible with [software/tool]?")
            patterns.Add("Can you provide a demo of [entity]?")
            patterns.Add("What are the prerequisites for using [entity]?")
            patterns.Add("How does the pricing model for [entity] work?")
            patterns.Add("What are the current trends in [entity]?")
            patterns.Add("What are the limitations of [entity]?")
            patterns.Add("What are the best practices for using [entity]?")
            patterns.Add("What are the common use cases for [entity]?")
            patterns.Add("How does [entity] handle [specific scenario]?")
            patterns.Add("What are the key performance indicators for [entity]?")
            patterns.Add("How is data privacy ensured in [entity]?")
            patterns.Add("What are the prerequisites for implementing [entity]?")
            patterns.Add("What are the data storage options for [entity]?")
            patterns.Add("What are the customization capabilities of [entity]?")
            patterns.Add("What are the integration options for [entity]?")
            patterns.Add("How is [entity] different from [competitor]?")
            patterns.Add("What are the customer reviews for [entity]?")
            patterns.Add("What is the target audience for [entity]?")
            patterns.Add("What are the key metrics to measure [entity] success?")
            patterns.Add("How does [entity] handle scalability?")
            patterns.Add("What are the deployment options for [entity]?")
            patterns.Add("What is the expected ROI of [entity]?")
            patterns.Add("What are the key challenges in implementing [entity]?")
            patterns.Add("How does [entity] ensure data security?")
            patterns.Add("What are the industry standards supported by [entity]?")
            patterns.Add("What is the reputation of [entity] in the market?")
            patterns.Add("How can I provide feedback or suggestions for [entity]?")
            patterns.Add("What are the training resources available for [entity]?")
            patterns.Add("What are the prerequisites for becoming a user of [entity]?")
            patterns.Add("How does [entity] handle data migration?")
            patterns.Add("What are the compliance regulations applicable to [entity]?")
            patterns.Add("What are the maintenance requirements for [entity]?")
            patterns.Add("What is the uptime guarantee for [entity]?")
            patterns.Add("How does [entity] handle error handling?")
            patterns.Add("What are the system integration requirements for [entity]?")
            patterns.Add("What are the support options available for [entity]?")
            patterns.Add("How can I request a feature enhancement for [entity]?")
            patterns.Add("What are the service level agreements for [entity]?")
            patterns.Add("What are the data analytics capabilities of [entity]?")
            patterns.Add("What is the data retention policy of [entity]?")
            patterns.Add("How does [entity] ensure high availability?")
            patterns.Add("What are the backup and disaster recovery mechanisms of [entity]?")
            patterns.Add("How does [entity] handle user access control?")
            patterns.Add("What are the reporting and analytics features of [entity]?")
            patterns.Add("What are the mobile app support options for [entity]?")
            patterns.Add("How does [entity] handle data synchronization?")
            patterns.Add("What are the data export capabilities of [entity]?")
            patterns.Add("What are the supported payment options for [entity]?")
            patterns.Add("How does [entity] handle user authentication?")
            patterns.Add("What are the content management features of [entity]?")
            patterns.Add("What are the collaboration capabilities of [entity]?")
            patterns.Add("How does [entity] handle version control?")
            patterns.Add("What are the document management capabilities of [entity]?")
            patterns.Add("What are the project management features of [entity]?")
            patterns.Add("How does [entity] handle workflow automation?")
            patterns.Add("What are the email integration options for [entity]?")
            patterns.Add("What are the social media integration capabilities of [entity]?")
            patterns.Add("How does [entity] handle customer support?")
            patterns.Add("What are the CRM (Customer Relationship Management) features of [entity]?")
            patterns.Add("What are the marketing automation capabilities of [entity]?")
            patterns.Add("How does [entity] handle data import?")
            patterns.Add("What are the inventory management features of [entity]?")
            patterns.Add("What are the sales forecasting capabilities of [entity]?")
            patterns.Add("How does [entity] handle order management?")
            patterns.Add("What are the HR (Human Resources) management features of [entity]?")
            patterns.Add("What are the accounting and finance capabilities of [entity]?")
            patterns.Add("How does [entity] handle customer data?")
            patterns.Add("What are the business intelligence features of [entity]?")

            ' Biology Questions
            patterns.Add("How does [organism] reproduce?")
            patterns.Add("What is the process of photosynthesis?")
            patterns.Add("What are the different parts of a cell?")
            patterns.Add("How does the digestive system work?")
            patterns.Add("What is DNA and how does it determine traits?")
            patterns.Add("How do plants make food?")
            patterns.Add("What is the role of the respiratory system?")
            patterns.Add("What are the stages of the life cycle?")
            patterns.Add("How does the circulatory system work?")
            patterns.Add("What are some examples of ecosystems?")
            patterns.Add("What is the classification of [animal/plant]?")
            patterns.Add("How does the nervous system function?")
            patterns.Add("What are the different types of animals?")
            patterns.Add("What is the importance of biodiversity?")
            patterns.Add("How do vaccines work?")
            patterns.Add("What is the process of evolution?")
            patterns.Add("How do plants and animals adapt to their environment?")
            patterns.Add("What are some examples of food chains?")
            patterns.Add("What is the role of DNA in heredity?")
            patterns.Add("How do different animals communicate?")

            ' Computer Science Questions
            patterns.Add("What is programming and how does it work?")
            patterns.Add("What are the different programming languages?")
            patterns.Add("How does a computer store and process information?")
            patterns.Add("What is an algorithm and how is it used?")
            patterns.Add("What are the basic concepts of coding?")
            patterns.Add("How do computers communicate with each other?")
            patterns.Add("What are the different types of computer networks?")
            patterns.Add("What is cybersecurity and why is it important?")
            patterns.Add("How do computers generate and display graphics?")
            patterns.Add("What is artificial intelligence and how is it used?")
            patterns.Add("What are the components of a computer system?")
            patterns.Add("How does data encryption work?")
            patterns.Add("What is the role of software in a computer?")
            patterns.Add("How are computer games developed?")
            patterns.Add("What is the Internet and how does it work?")
            patterns.Add("What is the difference between hardware and software?")
            patterns.Add("How do search engines like Google work?")
            patterns.Add("What are the different types of computer memory?")
            patterns.Add("What is cloud computing and how is it used?")
            patterns.Add("How do robots work and what are their applications?")

            ' NLP Questions
            patterns.Add("What is natural language processing?")
            patterns.Add("How do chatbots understand and respond to text?")
            patterns.Add("What are the applications of NLP in everyday life?")
            patterns.Add("How does voice recognition technology work?")
            patterns.Add("What is sentiment analysis and how is it used?")
            patterns.Add("What are the challenges in machine translation?")
            patterns.Add("How do spam filters detect and block unwanted emails?")
            patterns.Add("What is named entity recognition and why is it important?")
            patterns.Add("How does text summarization work?")
            patterns.Add("What are the steps involved in text classification?")
            patterns.Add("What is topic modeling and how is it used?")
            patterns.Add("How do search engines like Google understand user queries?")
            patterns.Add("What is part-of-speech tagging and why is it useful?")
            patterns.Add("How does sentiment analysis analyze emotions in text?")
            patterns.Add("What are the different techniques used in text generation?")
            patterns.Add("What is information extraction and how is it performed?")
            patterns.Add("How do language models like GPT-3 work?")
            patterns.Add("What are the ethical considerations in NLP?")
            patterns.Add("How can NLP be used for language translation?")
            patterns.Add("What are the limitations of current NLP systems?")

            patterns.Add("When did [organism] [action]?")
            patterns.Add("What is the [attribute] of [organism]?")
            patterns.Add("How does [organism] [action]?")
            patterns.Add("Who is [organism]'s [relation]?")
            patterns.Add("Where does [organism] [action]?")
            patterns.Add("Why did [organism] [action]?")
            patterns.Add("How many [organisms] are there?")
            patterns.Add("Which [organism] [action]?")

            patterns.Add("When did [concept] [action]?")
            patterns.Add("What is the [attribute] of [concept]?")
            patterns.Add("How does [concept] [action]?")
            patterns.Add("Who does [concept] [action]?")
            patterns.Add("Where does [concept] [action]?")
            patterns.Add("Why does [concept] [action]?")
            patterns.Add("How many [concepts] are there?")
            patterns.Add("Which [concept] [action]?")
            patterns.Add("When did [organism] [action]?")
            patterns.Add("What is the [attribute] of [organism]?")
            patterns.Add("How does [organism] [action]?")
            patterns.Add("Who is [organism]'s [relation]?")
            patterns.Add("Where does [organism] [action]?")
            patterns.Add("Why did [organism] [action]?")
            patterns.Add("How many [organisms] are there?")
            patterns.Add("Which [organism] [action]?")
            patterns.Add("When did [technology] [action]?")
            patterns.Add("What is the [attribute] of [technology]?")
            patterns.Add("How does [technology] [action]?")
            patterns.Add("Who uses [technology]?")
            patterns.Add("Where is [technology] used?")
            patterns.Add("Why is [technology] important?")
            patterns.Add("How many [technologies] are there?")
            patterns.Add("Which [technology] [action]?")
            ' Biology Questions
            patterns.Add("How does [organism] reproduce?")
            patterns.Add("What is the process of photosynthesis?")
            patterns.Add("What are the different parts of a cell?")
            patterns.Add("How does the digestive system work?")
            patterns.Add("What is DNA and how does it determine traits?")
            patterns.Add("How do plants make food?")
            patterns.Add("What is the role of the respiratory system?")
            patterns.Add("What are the stages of the life cycle?")
            patterns.Add("How does the circulatory system work?")
            patterns.Add("What are some examples of ecosystems?")
            patterns.Add("What is the classification of [animal/plant]?")
            patterns.Add("How does the nervous system function?")
            patterns.Add("What are the different types of animals?")
            patterns.Add("What is the importance of biodiversity?")
            patterns.Add("How do vaccines work?")
            patterns.Add("What is the process of evolution?")
            patterns.Add("How do plants and animals adapt to their environment?")
            patterns.Add("What are some examples of food chains?")
            patterns.Add("What is the role of DNA in heredity?")
            patterns.Add("How do different animals communicate?")

            ' Computer Science Questions
            patterns.Add("What is programming and how does it work?")
            patterns.Add("What are the different programming languages?")
            patterns.Add("How does a computer store and process information?")
            patterns.Add("What is an algorithm and how is it used?")
            patterns.Add("What are the basic concepts of coding?")
            patterns.Add("How do computers communicate with each other?")
            patterns.Add("What are the different types of computer networks?")
            patterns.Add("What is cybersecurity and why is it important?")
            patterns.Add("How do computers generate and display graphics?")
            patterns.Add("What is artificial intelligence and how is it used?")
            patterns.Add("What are the components of a computer system?")
            patterns.Add("How does data encryption work?")
            patterns.Add("What is the role of software in a computer?")
            patterns.Add("How are computer games developed?")
            patterns.Add("What is the Internet and how does it work?")
            patterns.Add("What is the difference between hardware and software?")
            patterns.Add("How do search engines like Google work?")
            patterns.Add("What are the different types of computer memory?")
            patterns.Add("What is cloud computing and how is it used?")
            patterns.Add("How do robots work and what are their applications?")

            ' NLP Questions
            patterns.Add("What is natural language processing?")
            patterns.Add("How do chatbots understand and respond to text?")
            patterns.Add("What are the applications of NLP in everyday life?")
            patterns.Add("How does voice recognition technology work?")
            patterns.Add("What is sentiment analysis and how is it used?")
            patterns.Add("What are the challenges in machine translation?")
            patterns.Add("How do spam filters detect and block unwanted emails?")
            patterns.Add("What is named entity recognition and why is it important?")
            patterns.Add("How does text summarization work?")
            patterns.Add("What are the steps involved in text classification?")
            patterns.Add("What is topic modeling and how is it used?")
            patterns.Add("How do search engines like Google understand user queries?")
            patterns.Add("What is part-of-speech tagging and why is it useful?")
            patterns.Add("How does sentiment analysis analyze emotions in text?")
            patterns.Add("What are the different techniques used in text generation?")
            patterns.Add("What is information extraction and how is it performed?")
            patterns.Add("How do language models like GPT-3 work?")
            patterns.Add("What are the ethical considerations in NLP?")
            patterns.Add("How can NLP be used for language translation?")
            patterns.Add("What are the limitations of current NLP systems?")

            patterns.Add("When did [entity] [action]?")
            patterns.Add("What is the [attribute] of [entity]?")
            patterns.Add("How does [entity] [action]?")
            patterns.Add("Who is [entity]'s [relation]?")
            patterns.Add("Where does [entity] [action]?")
            patterns.Add("Why did [entity] [action]?")
            patterns.Add("How many [entity]s are there?")
            patterns.Add("Which [entity] [action]?")

            patterns.Add("The formula for calculating [attribute] is [formula].")
            patterns.Add("[Entity] works by [explanation].")
            patterns.Add("The basic principles of [entity] are [principles].")
            patterns.Add("The difference between [entity1] and [entity2] is [difference].")
            patterns.Add("[Entity] affects our daily life by [impact].")
            patterns.Add("The importance of [entity] in [subject] is [importance].")
            patterns.Add("Some real-world applications of [entity] are [applications].")
            patterns.Add("Scientists study [entity] by [study method].")
            patterns.Add("Some interesting facts about [entity] are [facts].")
            patterns.Add("[Concept] can be explained in simple terms as [explanation].")
            patterns.Add("The different types of [entity] include [types].")
            patterns.Add("[Entity] helps us understand [concept] by [explanation].")
            patterns.Add("The importance of [concept] in [subject] is [importance].")
            patterns.Add("The steps involved in [process] are [steps].")
            patterns.Add("Some examples of [entity] in everyday life are [examples].")
            patterns.Add("The main components of [entity] are [components].")
            patterns.Add("[Entity] relates to [subject] by [relation].")
            patterns.Add("When [action] [entity], [result] occurs.")
            patterns.Add("The advantages of [entity] are [advantages], and the disadvantages are [disadvantages].")
            patterns.Add("To perform [action] with [entity], you can [method].")
            patterns.Add("The key features of [entity] include [features].")
            patterns.Add("The concept of [entity] can be explained using an analogy: [analogy].")
            patterns.Add("Some safety precautions when dealing with [entity] are [precautions].")
            patterns.Add("[Entity] can be compared to [similar entity] based on [comparison].")
            patterns.Add("The properties of [entity] include [properties].")
            patterns.Add("[Entity] interacts with [other entity] through [interaction].")
            patterns.Add("Studying [subject] is important because [reasons].")
            patterns.Add("The limitations of [entity] are [limitations].")
            patterns.Add("The practical uses of [concept] are [uses].")
            patterns.Add("[Entity] impacts the environment by [impact].")
            patterns.Add("The concept of [entity] can be explained through an experiment: [experiment].")
            patterns.Add("Future advancements in [subject] may include [advancements].")
            patterns.Add("[Entity] contributes to sustainable development by [contribution].")
            patterns.Add("Ethical considerations related to [entity] include [considerations].")
            patterns.Add("To solve [problem] using [entity], you can [solution].")
            patterns.Add("The challenges faced in studying [subject] include [challenges].")
            patterns.Add("[Entity] supports scientific research by [support].")
            patterns.Add("Some key discoveries or inventions related to [subject] are [discoveries].")
            patterns.Add("We can use [entity] to improve our daily lives by [improvement].")
            patterns.Add("The different branches or fields within [subject] are [branches].")
            patterns.Add("Common misconceptions about [entity] include [misconceptions].")
            patterns.Add("[Entity] impacts the economy by [impact].")
            patterns.Add("A step-by-step explanation of [process] is as follows: [explanation].")
            patterns.Add("The implications of [entity] on society include [implications].")
            patterns.Add("[Entity] contributes to technological advancements by [contribution].")
            patterns.Add("Current research areas in [subject] include [research areas].")
            patterns.Add("The concept of [entity] can be visualized using [visual aids].")
            patterns.Add("Career opportunities in [subject] include [careers].")

            ' Biology Answers
            patterns.Add("[Organism] reproduces through [reproductive process].")
            patterns.Add("Photosynthesis is the process by which [plants] convert sunlight into [energy].")
            patterns.Add("The different parts of a cell include [cell components].")
            patterns.Add("The digestive system works by [digestive process].")
            patterns.Add("DNA is a molecule that contains genetic information and determines [traits].")
            patterns.Add("Plants make food through a process called [photosynthesis].")
            patterns.Add("The respiratory system is responsible for [respiratory function].")
            patterns.Add("The stages of the life cycle are [life cycle stages].")
            patterns.Add("The circulatory system works by [circulatory process].")
            patterns.Add("Examples of ecosystems include [ecosystem examples].")
            patterns.Add("The classification of [animal/plant] is [classification].")
            patterns.Add("The nervous system functions by [nervous system process].")
            patterns.Add("There are different types of animals such as [animal types].")
            patterns.Add("Biodiversity is important for [reasons].")
            patterns.Add("Vaccines work by [vaccine mechanism].")
            patterns.Add("Evolution is the process of [evolution process].")
            patterns.Add("Plants and animals adapt to their environment through [adaptation process].")
            patterns.Add("Examples of food chains include [food chain examples].")
            patterns.Add("DNA plays a role in heredity by [heredity mechanism].")
            patterns.Add("Different animals communicate through [animal communication methods].")

            ' Computer Science Answers
            patterns.Add("Programming is the process of creating instructions for a computer to follow.")
            patterns.Add("There are different programming languages such as [programming languages].")
            patterns.Add("Computers store and process information using [computer mechanism].")
            patterns.Add("An algorithm is a step-by-step set of instructions for solving a problem.")
            patterns.Add("Coding involves using programming languages to write instructions for computers.")
            patterns.Add("Computers communicate with each other through [communication methods].")
            patterns.Add("There are different types of computer networks such as [network types].")
            patterns.Add("Cybersecurity is important to protect computer systems from [security threats].")
            patterns.Add("Computers generate and display graphics using [graphics technology].")
            patterns.Add("Artificial intelligence is used to create intelligent computer systems.")
            patterns.Add("The components of a computer system include [computer components].")
            patterns.Add("Data encryption ensures that data is secure and cannot be accessed by unauthorized parties.")
            patterns.Add("Software is a collection of programs that perform specific tasks on a computer.")
            patterns.Add("Computer games are developed using [game development techniques].")
            patterns.Add("The Internet is a global network of computers that are connected and can share information.")
            patterns.Add("Hardware refers to the physical components of a computer, while software refers to programs and data.")
            patterns.Add("Search engines like Google use algorithms to index and retrieve information from the web.")
            patterns.Add("Computer memory includes [memory types] used to store data and instructions.")
            patterns.Add("Cloud computing allows users to access and store data and applications on remote servers.")
            patterns.Add("Robots work through a combination of [robotics technology] and are used in various applications.")

            ' NLP Answers
            patterns.Add("Natural language processing is a field of study that focuses on how computers understand and generate human language.")
            patterns.Add("Chatbots understand and respond to text by analyzing the input and selecting appropriate responses.")
            patterns.Add("NLP has applications in everyday life such as [NLP applications].")
            patterns.Add("Voice recognition technology converts spoken language into written text.")
            patterns.Add("Sentiment analysis is used to analyze and understand emotions expressed in text.")
            patterns.Add("Machine translation involves translating text from one language to another using computer algorithms.")
            patterns.Add("Spam filters detect and block unwanted emails based on [spam detection techniques].")
            patterns.Add("Named entity recognition is the process of identifying and classifying named entities in text.")
            patterns.Add("Text summarization involves condensing a piece of text into a shorter version while retaining the main points.")
            patterns.Add("Text classification is the task of assigning predefined categories or labels to text based on its content.")
            patterns.Add("Topic modeling is a technique used to discover abstract topics in a collection of documents.")
            patterns.Add("Search engines like Google understand user queries by analyzing the keywords and context.")
            patterns.Add("Part-of-speech tagging assigns grammatical tags to words in a sentence to understand their roles.")
            patterns.Add("Text generation techniques include [text generation methods] used to create human-like text.")
            patterns.Add("Information extraction involves identifying and extracting structured information from unstructured text.")
            patterns.Add("Language models like GPT-3 use deep learning algorithms to generate coherent and contextually relevant text.")
            patterns.Add("Ethical considerations in NLP include [NLP ethical considerations] to ensure fair and unbiased use of the technology.")
            patterns.Add("NLP can be used for language translation by applying machine learning techniques to analyze and generate text.")
            patterns.Add("Current NLP systems have limitations in [NLP limitations].")

            patterns.Add("[Organism] [action] at [time].")
            patterns.Add("The [attribute] of [organism] is [value].")
            patterns.Add("[Organism] [action] by [method].")
            patterns.Add("[Organism]'s [relation] is [person].")
            patterns.Add("[Organism] [action] in [location].")
            patterns.Add("[Organism] [action] because [reason].")
            patterns.Add("There are [number] [organisms].")
            patterns.Add("[Organism] [action] [criteria].")
            patterns.Add("[Organism] [action] at [time].")
            patterns.Add("The [attribute] of [organism] is [value].")
            patterns.Add("[Organism] [action] by [method].")
            patterns.Add("[Organism]'s [relation] is [person].")
            patterns.Add("[Organism] [action] in [location].")
            patterns.Add("[Organism] [action] because [reason].")
            patterns.Add("There are [number] [organisms].")
            patterns.Add("[Organism] [action] [criteria].")

            patterns.Add("[Concept] [action] at [time].")
            patterns.Add("The [attribute] of [concept] is [value].")
            patterns.Add("[Concept] [action] by [method].")
            patterns.Add("[Concept] is used by [person].")
            patterns.Add("[Concept] [action] in [location].")
            patterns.Add("[Concept] [action] because [reason].")
            patterns.Add("There are [number] [concepts].")
            patterns.Add("[Concept] [action] [criteria].")

            patterns.Add("[Technology] [action] at [time].")
            patterns.Add("The [attribute] of [technology] is [value].")
            patterns.Add("[Technology] [action] by [method].")
            patterns.Add("[Person] uses [technology].")
            patterns.Add("[Technology] is used in [location].")
            patterns.Add("[Technology] is important because [reason].")
            patterns.Add("There are [number] [technologies].")
            patterns.Add("[Technology] [action] [criteria].")

            ' Biology Answers
            patterns.Add("[Organism] reproduces through [reproductive process].")
            patterns.Add("Photosynthesis is the process by which [plants] convert sunlight into [energy].")
            patterns.Add("The different parts of a cell include [cell components].")
            patterns.Add("The digestive system works by [digestive process].")
            patterns.Add("DNA is a molecule that contains genetic information and determines [traits].")
            patterns.Add("Plants make food through a process called [photosynthesis].")
            patterns.Add("The respiratory system is responsible for [respiratory function].")
            patterns.Add("The stages of the life cycle are [life cycle stages].")
            patterns.Add("The circulatory system works by [circulatory process].")
            patterns.Add("Examples of ecosystems include [ecosystem examples].")
            patterns.Add("The classification of [animal/plant] is [classification].")
            patterns.Add("The nervous system functions by [nervous system process].")
            patterns.Add("There are different types of animals such as [animal types].")
            patterns.Add("Biodiversity is important for [reasons].")
            patterns.Add("Vaccines work by [vaccine mechanism].")
            patterns.Add("Evolution is the process of [evolution process].")
            patterns.Add("Plants and animals adapt to their environment through [adaptation process].")
            patterns.Add("Examples of food chains include [food chain examples].")
            patterns.Add("DNA plays a role in heredity by [heredity mechanism].")
            patterns.Add("Different animals communicate through [animal communication methods].")

            ' Computer Science Answers
            patterns.Add("Programming is the process of creating instructions for a computer to follow.")
            patterns.Add("There are different programming languages such as [programming languages].")
            patterns.Add("Computers store and process information using [computer mechanism].")
            patterns.Add("An algorithm is a step-by-step set of instructions for solving a problem.")
            patterns.Add("Coding involves using programming languages to write instructions for computers.")
            patterns.Add("Computers communicate with each other through [communication methods].")
            patterns.Add("There are different types of computer networks such as [network types].")
            patterns.Add("Cybersecurity is important to protect computer systems from [security threats].")
            patterns.Add("Computers generate and display graphics using [graphics technology].")
            patterns.Add("Artificial intelligence is used to create intelligent computer systems.")
            patterns.Add("The components of a computer system include [computer components].")
            patterns.Add("Data encryption ensures that data is secure and cannot be accessed by unauthorized parties.")
            patterns.Add("Software is a collection of programs that perform specific tasks on a computer.")
            patterns.Add("Computer games are developed using [game development techniques].")
            patterns.Add("The Internet is a global network of computers that are connected and can share information.")
            patterns.Add("Hardware refers to the physical components of a computer, while software refers to programs and data.")
            patterns.Add("Search engines like Google use algorithms to index and retrieve information from the web.")
            patterns.Add("Computer memory includes [memory types] used to store data and instructions.")
            patterns.Add("Cloud computing allows users to access and store data and applications on remote servers.")
            patterns.Add("Robots work through a combination of [robotics technology] and are used in various applications.")

            ' NLP Answers
            patterns.Add("Natural language processing is a field of study that focuses on how computers understand and generate human language.")
            patterns.Add("Chatbots understand and respond to text by analyzing the input and selecting appropriate responses.")
            patterns.Add("NLP has applications in everyday life such as [NLP applications].")
            patterns.Add("Voice recognition technology converts spoken language into written text.")
            patterns.Add("Sentiment analysis is used to analyze and understand emotions expressed in text.")
            patterns.Add("Machine translation involves translating text from one language to another using computer algorithms.")
            patterns.Add("Spam filters detect and block unwanted emails based on [spam detection techniques].")
            patterns.Add("Named entity recognition is the process of identifying and classifying named entities in text.")
            patterns.Add("Text summarization involves condensing a piece of text into a shorter version while retaining the main points.")
            patterns.Add("Text classification is the task of assigning predefined categories or labels to text based on its content.")
            patterns.Add("Topic modeling is a technique used to discover abstract topics in a collection of documents.")
            patterns.Add("Search engines like Google understand user queries by analyzing the keywords and context.")
            patterns.Add("Part-of-speech tagging assigns grammatical tags to words in a sentence to understand their roles.")
            patterns.Add("Text generation techniques include [text generation methods] used to create human-like text.")
            patterns.Add("Information extraction involves identifying and extracting structured information from unstructured text.")
            patterns.Add("Language models like GPT-3 use deep learning algorithms to generate coherent and contextually relevant text.")
            patterns.Add("Ethical considerations in NLP include [NLP ethical considerations] to ensure fair and unbiased use of the technology.")
            patterns.Add("NLP can be used for language translation by applying machine learning techniques to analyze and generate text.")
            patterns.Add("Current NLP systems have limitations in [NLP limitations].")

            patterns.Add("[Entity] [action] at [time].")
            patterns.Add("The [attribute] of [entity] is [value].")
            patterns.Add("[Entity] [action] by [method].")
            patterns.Add("[Entity]'s [relation] is [person].")
            patterns.Add("[Entity] [action] in [location].")
            patterns.Add("[Entity] [action] because [reason].")
            patterns.Add("There are [number] [entity]s.")
            patterns.Add("[Entity] [action] [criteria].")

            Return patterns
        End Function
        ''' <summary>
        ''' When no lists are available, A mixed corpus of documents 
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function GetDocumentCorpus() As List(Of String)
            ' Load paragraphs based on different topics
            Dim paragraphs As New List(Of String)

            ' Computer Science Topics
            paragraphs.Add("Computer Science is the study of computation and information processing.")
            paragraphs.Add("Algorithms and data structures are fundamental concepts in computer science.")
            paragraphs.Add("Computer networks enable communication and data exchange between devices.")
            paragraphs.Add("Artificial Intelligence is a branch of computer science that focuses on creating intelligent machines.")
            paragraphs.Add("Software engineering is the discipline of designing, developing, and maintaining software systems.")

            ' NLP Topics
            paragraphs.Add("Natural Language Processing (NLP) is a subfield of artificial intelligence.")
            paragraphs.Add("NLP techniques enable computers to understand, interpret, and generate human language.")
            paragraphs.Add("Named Entity Recognition (NER) is a common task in NLP.")
            paragraphs.Add("Machine Translation is the task of translating text from one language to another.")
            paragraphs.Add("Sentiment analysis aims to determine the sentiment or opinion expressed in a piece of text.")

            paragraphs.Add("The quick brown fox jumps over the lazy dog.")
            paragraphs.Add("The cat and the dog are best friends.")
            paragraphs.Add("Programming languages are used to write computer programs.")
            paragraphs.Add("Natural Language Processing (NLP) is a subfield of artificial intelligence.")
            paragraphs.Add("Machine learning algorithms can be used for sentiment analysis.")
            ' Train the model on a corpus of text
            Dim trainingData As New List(Of String)
            trainingData.Add("Hello")
            trainingData.Add("Hi there")
            trainingData.Add("How are you?")
            trainingData.Add("What's up?")
            trainingData.Add("I'm doing well, thanks!")
            trainingData.Add("Not too bad, how about you?")
            trainingData.Add("Great! What can I help you with?")
            trainingData.Add("I need some assistance")
            trainingData.Add("Can you provide me with information?")
            trainingData.Add("Sure, what do you need?")
            trainingData.Add("Can you tell me about your services?")
            trainingData.Add("We offer a wide range of services to cater to your needs.")
            trainingData.Add("What are the payment options?")
            trainingData.Add("We accept all major credit cards and PayPal.")
            trainingData.Add("Do you have any ongoing promotions?")
            trainingData.Add("Yes, we have a special discount available for new customers.")
            trainingData.Add("How long does shipping take?")
            trainingData.Add("Shipping usually takes 3-5 business days.")
            trainingData.Add("What is your return policy?")
            trainingData.Add("We offer a 30-day return policy for unused items.")
            trainingData.Add("Can you recommend a good restaurant nearby?")
            trainingData.Add("Sure! There's a great Italian restaurant called 'La Bella Vita' just a few blocks away.")
            trainingData.Add("What movies are currently playing?")
            trainingData.Add("The latest releases include 'Avengers: Endgame' and 'The Lion King'.")
            trainingData.Add("What time does the museum open?")
            trainingData.Add("The museum opens at 9:00 AM.")
            trainingData.Add("How do I reset my password?")
            trainingData.Add("You can reset your password by clicking on the 'Forgot Password' link on the login page.")
            trainingData.Add("What are the system requirements for this software?")
            trainingData.Add("The system requirements are listed on our website under the 'Support' section.")
            trainingData.Add("Can you provide technical support?")
            trainingData.Add("Yes, we have a dedicated support team available 24/7 to assist you.")
            trainingData.Add("What is the best way to contact customer service?")
            trainingData.Add("You can reach our customer service team by phone, email, or live chat.")
            trainingData.Add("How do I cancel my subscription?")
            trainingData.Add("To cancel your subscription, please go to your account settings and follow the instructions.")
            trainingData.Add("What are the available colors for this product?")
            trainingData.Add("The available colors are red, blue, and green.")
            trainingData.Add("Do you offer international shipping?")
            trainingData.Add("Yes, we offer international shipping to select countries.")
            trainingData.Add("Can I track my order?")
            trainingData.Add("Yes, you can track your order by entering the tracking number on our website.")
            trainingData.Add("What is your privacy policy?")
            trainingData.Add("Our privacy policy can be found on our website under the 'Privacy' section.")
            trainingData.Add("How do I request a refund?")
            trainingData.Add("To request a refund, please contact our customer service team with your order details.")
            trainingData.Add("What are the opening hours?")
            trainingData.Add("We are open from Monday to Friday, 9:00 AM to 6:00 PM.")
            trainingData.Add("Is there a warranty for this product?")
            trainingData.Add("Yes, this product comes with a one-year warranty.")
            trainingData.Add("Can I schedule an appointment?")
            trainingData.Add("Yes, you can schedule an appointment by calling our office.")
            trainingData.Add("Do you have any vegetarian options?")
            trainingData.Add("Yes, we have a dedicated vegetarian menu.")
            trainingData.Add("What is your company's mission statement?")
            trainingData.Add("Our mission is to provide high-quality products and excellent customer service.")
            trainingData.Add("How can I apply for a job at your company?")
            trainingData.Add("You can apply for a job by submitting your resume through our online application form.")
            'movie dialogues
            trainingData.Add("Luke: I am your father.")
            trainingData.Add("Darth Vader: Noooo!")
            trainingData.Add("Han Solo: May the Force be with you.")
            trainingData.Add("Princess Leia: I love you.")
            trainingData.Add("Han Solo: I know.")
            trainingData.Add("Yoda: Do or do not. There is no try.")
            trainingData.Add("Obi-Wan Kenobi: You were the chosen one!")
            trainingData.Add("Anakin Skywalker: I hate you!")
            trainingData.Add("Marty McFly: Great Scott!")
            trainingData.Add("Doc Brown: Roads? Where we're going, we don't need roads.")
            trainingData.Add("Tony Stark: I am Iron Man.")
            trainingData.Add("Peter Parker: With great power comes great responsibility.")
            trainingData.Add("Bruce Wayne: I'm Batman.")
            trainingData.Add("Alfred Pennyworth: Why do we fall? So we can learn to pick ourselves up.")
            trainingData.Add("Sherlock Holmes: Elementary, my dear Watson.")
            trainingData.Add("Dr. John Watson: It is a capital mistake to theorize before one has data.")
            trainingData.Add("James Bond: The name's Bond. James Bond.")
            trainingData.Add("Harry Potter: I solemnly swear that I am up to no good.")
            trainingData.Add("Ron Weasley: Bloody hell!")
            trainingData.Add("Hermione Granger: It's LeviOsa, not LevioSA.")
            trainingData.Add("Gandalf: You shall not pass!")
            trainingData.Add("Frodo Baggins: I will take the ring, though I do not know the way.")
            trainingData.Add("Samwise Gamgee: I can't carry it for you, but I can carry you!")
            trainingData.Add("Dumbledore: Happiness can be found even in the darkest of times.")
            trainingData.Add("Severus Snape: Always.")


            paragraphs.AddRange(trainingData)

            Dim inputTexts As String() = {
                "John Doe is a software developer from New York. He specializes in Python programming.",
                "Mary Smith is an artist from Los Angeles. She loves to paint landscapes.",
                "Peter Johnson is a doctor from Chicago. He works at a local hospital.",
                "Sara Williams is a teacher from Boston. She teaches English literature.",
                "David Brown is a musician from Seattle. He plays the guitar in a band.",
                "I am a software developer with 5 years of experience. I have expertise in Python and Java.",
        "As a data scientist, I have a Ph.D. in Machine Learning and 8 years of experience.",
        "I am a web developer skilled in Java and Python. I have worked at Microsoft for 10 years.",
        "I am an electrical engineer with a Master's degree and 8 years of experience in power systems.",
        "As a nurse, I have a Bachelor's degree in Nursing and 5 years of experience in a hospital setting.",
        "I am a graphic designer with expertise in Adobe Photoshop and Illustrator. I have worked freelance for 5 years.",
        "As a teacher, I have a Bachelor's degree in Education and 8 years of experience in primary schools.",
        "I am a mechanical engineer with a Ph.D. in Robotics and 10 years of experience in autonomous systems.",
        "As a lawyer, I have a Juris Doctor degree and 5 years of experience in corporate law.",
        "I am a marketing specialist with expertise in digital marketing and social media management. I have worked at Google for 8 years.",
        "As a chef, I have culinary training and 5 years of experience in high-end restaurants.",
        "I am a financial analyst with a Master's degree in Finance and 8 years of experience in investment banking.",
        "I am a software developer with 5 years of experience. I have expertise in Python and Java.",
        "As a data scientist, I have a Ph.D. in Machine Learning and 8 years of experience.",
        "I am a web developer skilled in Java and Python. I have worked at Microsoft for 10 years.",
        "I am an electrical engineer with a Master's degree and 8 years of experience in power systems.",
        "As a nurse, I have a Bachelor's degree in Nursing and 5 years of experience in a hospital setting.",
        "I am a graphic designer with expertise in Adobe Photoshop and Illustrator. I have worked freelance for 5 years.",
        "As a teacher, I have a Bachelor's degree in Education and 8 years of experience in primary schools.",
        "I am a mechanical engineer with a Ph.D. in Robotics and 10 years of experience in autonomous systems.",
        "As a lawyer, I have a Juris Doctor degree and 5 years of experience in corporate law.",
        "I am a marketing specialist with expertise in digital marketing and social media management. I have worked at Google for 8 years.",
        "As a chef, I have culinary training and 5 years of experience in high-end restaurants.",
        "I am a financial analyst with a Master's degree in Finance and 8 years of experience in investment banking.",
        "I am a software developer with 5 years of experience. I have expertise in Python and Java.",
        "As a data scientist, I have a Ph.D. in Machine Learning and 8 years of experience.",
        "I am a web developer skilled in Java and Python. I have worked at Microsoft for 10 years.",
        "I am an electrical engineer with a Master's degree and 8 years of experience in power systems.",
        "As a nurse, I have a Bachelor's degree in Nursing and 5 years of experience in a hospital setting.",
        "I am a graphic designer with expertise in Adobe Photoshop and Illustrator. I have worked freelance for 5 years.",
        "As a teacher, I have a Bachelor's degree in Education and 8 years of experience in primary schools.",
        "I am a mechanical engineer with a Ph.D. in Robotics and 10 years of experience in autonomous systems.",
        "As a lawyer, I have a Juris Doctor degree and 5 years of experience in corporate law.",
        "I am a marketing specialist with expertise in digital marketing and social media management. I have worked at Google for 8 years.",
        "As a chef, I have culinary training and 5 years of experience in high-end restaurants.",
        "I am a financial analyst with a Master's degree in Finance and 8 years of experience in investment banking."
    }
            paragraphs.AddRange(inputTexts)
            Dim NLP As String = "Natural language processing (NLP) Is a field Of artificial intelligence that focuses On the interaction between computers And humans Using natural language. It combines linguistics, computer science, And machine learning To enable computers To understand, interpret, And generate human language.

Machine learning is a subset of artificial intelligence that deals with the development of algorithms and models that allow computers to learn and make predictions or decisions without being explicitly programmed. It plays a crucial role in various applications, including NLP.

In recent news, researchers at XYZ University have developed a new deep learning algorithm for sentiment analysis in NLP. The algorithm achieved state-of-the-art results on multiple datasets and has the potential to improve various NLP tasks.

Another significant development in the computer science industry is the introduction of GPT-3, a powerful language model developed by OpenAI. GPT-3 utilizes advanced machine learning techniques to generate human-like text and has shown promising results in various language-related tasks.

Key people in the data science and AI industry include Andrew Ng, the founder of deeplearning.ai and a prominent figure in the field of machine learning, and Yann LeCun, the director of AI Research at Facebook and a pioneer in deep learning.

These are just a few examples of the vast field of NLP, machine learning, and the latest developments in the computer science industry."
            paragraphs.Add(NLP)

            Return paragraphs
        End Function
    End Class


End Namespace
