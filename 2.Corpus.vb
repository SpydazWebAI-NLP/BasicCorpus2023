Imports System.Text.RegularExpressions
Imports System.Web.Script.Serialization

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

Namespace Vocabulary

    ''' <summary>
    ''' Corpus Language Model
    ''' Used to HoldDocuments : a corpus of documents Calculating detecting the 
    ''' known entitys and topics in the model; 
    ''' A known list of Entitys and Topics are required to create this model
    ''' This language model is ideally suited for NER / and other corpus interogations
    ''' 
    ''' </summary>
    Public Class Corpus
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

End Namespace
