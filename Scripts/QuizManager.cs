using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizManager : MonoBehaviour
{
#pragma warning disable 649
    //ref to the QuizGameUI script
    [SerializeField] private QuizGameUI quizGameUI;
    //ref to the scriptableobject file
    [SerializeField] private List<QuizDataScriptable> quizDataList;
    [SerializeField] private float timeInSeconds;
#pragma warning restore 649

    private string currentCategory = "";
    private int correctAnswerCount = 0;
    //dados das perguntas
    private List<Question> questions;
    //dados da pergunta atual
    private Question selectedQuetion = new Question();
    private int gameScore;
    private int lifesRemaining;
    private float currentTime;
    private QuizDataScriptable dataScriptable;

    private GameStatus gameStatus = GameStatus.NEXT;

    public GameStatus GameStatus { get { return gameStatus; } }

    public List<QuizDataScriptable> QuizData { get => quizDataList; }

    public void StartGame(int categoryIndex, string category)
    {
        currentCategory = category; //categoria atual
        correctAnswerCount = 0; //contagem de respostas corretas
        gameScore = 0; //pontuação do jogo
        lifesRemaining = 3; //jogador inicia com 3 vidas
        currentTime = timeInSeconds;
        //definindo os dados das perguntas
        questions = new List<Question>();
        dataScriptable = quizDataList[categoryIndex];
        questions.AddRange(dataScriptable.questions);
        //chama a função para selecionar a questão
        SelectQuestion();
        gameStatus = GameStatus.PLAYING;
    }

    private void SelectQuestion()
    {
        //pega o número da questão aleatória para informar na tela
        int val = UnityEngine.Random.Range(0, questions.Count);
        //define a questão selecionada 
        selectedQuetion = questions[val];
        //envia a pergunta e apresenta na tela
        quizGameUI.SetQuestion(selectedQuetion);

        questions.RemoveAt(val);
    }

    private void Update()
    {
        if (gameStatus == GameStatus.PLAYING)
        {
            currentTime -= Time.deltaTime;
            SetTime(currentTime);
        }
    }

    void SetTime(float value)
    {
        TimeSpan time = TimeSpan.FromSeconds(currentTime); //definido o valor do tempo
        quizGameUI.TimerText.text = time.ToString("mm':'ss"); //converter hora para o formato correto

        if (currentTime <= 0)
        {
            //game over
            GameEnd();
        }
    }

    public bool Answer(string selectedOption)
    {
        //define o padrão como falso
        bool correct = false;
        //se a resposta selecionada for igual a resposta correta
        if (selectedQuetion.correctAns == selectedOption)
        {
            //correta = incrementa o score para +50
            correctAnswerCount++;
            correct = true;
            gameScore += 50;
            quizGameUI.ScoreText.text = "Score:" + gameScore; //texto que consta na tela do score para o jogador, incrementando a cada resposta correta +50 pontos
        }
        else
        {
            //caso a resposta estiver errada, é eliminada uma vida e não conta no score
            lifesRemaining--;
            quizGameUI.ReduceLife(lifesRemaining); //reduz um ponto na vida (total de 3 vidas no início de cada partida)

            //quando as vidas terminarem, quando chegar a 0, finaliza o jogo
            if (lifesRemaining == 0)
            {
                GameEnd();
            }
        }

        if (gameStatus == GameStatus.PLAYING)
        {
            if (questions.Count > 0)
            {
                //pontuaçao > 0 chama o método para selecionar a questão novamente após 1s
                Invoke("SelectQuestion", 0.4f);
            }
        
            else
            {
                GameEnd();
            }
        }
        //retorna o valor do bool correto
        return correct;
    }

    private void GameEnd()
    {
        gameStatus = GameStatus.NEXT;
        quizGameUI.GameOverPanel.SetActive(true);

        //comparando a pontuação atual com a pontuação salva e salvando a nova pontuação
        //se correctAnswerCount > PlayerPrefs.GetInt(currentCategory) salva o score

        //salvando o score
        PlayerPrefs.SetInt(currentCategory, correctAnswerCount); //salva o score para a categoria atual
    }
}

//informações para armazenar os dados das perguntas que serão preenchidas na unity
[System.Serializable] //conseguimos editar os dados externamente
public class Question
{
    public string questionInfo;         //texto da questão
    public QuestionType questionType;   //tipo do texto (imagem texto audio ou vídeo)
    public Sprite questionImage;        //escolher a imagem para o texto
    public AudioClip audioClip;         //selecionar audio para o texto
    public UnityEngine.Video.VideoClip videoClip;  //selecionar video para o texto
    public List<string> options;        //opções para selecionar
    public string correctAns;           //opção correta
}

[System.Serializable]
public enum QuestionType
{
    TEXT, IMAGE, AUDIO, VIDEO //tipos para escolher na hora de inserir o tipo da pergunta na unity 
}

[SerializeField] //deixar visível ao editor 
public enum GameStatus 
{
    PLAYING, NEXT
}