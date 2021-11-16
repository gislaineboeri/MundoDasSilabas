using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; //vincular outros scripts no atual

public class QuizGameUI : MonoBehaviour
{
    #pragma warning disable 649
        [SerializeField] private QuizManager quizManager; //referência para o script do Quiz Manager
        [SerializeField] private CategoryBtnScript categoryBtnPrefab; //botão para as categorias
        [SerializeField] private GameObject scrollHolder; //campo para as perguntas
        [SerializeField] private Text scoreText, timerText; //texto para score e tempo
        [SerializeField] private List<Image> lifeImageList; //lista para imagens que representa a quantidade de vidas
        [SerializeField] private GameObject gameOverPanel, mainMenu, gamePanel;
        [SerializeField] private Color correctCol, wrongCol, normalCol; //cor dos botões para quando acertar a resposta, errar e a cor do botão atual/normal
        [SerializeField] private Image questionImg; //permite inserir imagens
        [SerializeField] private UnityEngine.Video.VideoPlayer questionVideo; //permite mostrar vídeos
        [SerializeField] private AudioSource questionAudio; //para inserir audio
        [SerializeField] private Text questionInfoText; //texto para mostrar a questão
        [SerializeField] private List<Button> options; //referência dos botões de opções (respostas) para inserir dentro do QuizUIManager

#pragma warning restore 649 //desativando um ativo do compilador

    private float audioLength; //armazenar o áudio
    private Question question; //armazenar dados da pergunta atual
    private bool answered = false; //utilizando o bool para verificar se foi respondido a questão ou não  

    public Text TimerText { get => timerText; }                     
    public Text ScoreText { get => scoreText; }                    
    public GameObject GameOverPanel { get => gameOverPanel; }                    

    private void Start()
    {
         //verifica qual botão categoria o usuário selecionou
        for (int i = 0; i < options.Count; i++)
        {
            Button localBtn = options[i];
            localBtn.onClick.AddListener(() => OnClick(localBtn));
        }

        CreateCategoryButtons();

    }
    
    public void SetQuestion(Question question)
    {
        //definindo a o tipo da questão a ser escolhida
        this.question = question;
        //verificando o tipo da questão, audio texto imagem ou vídeo
        switch (question.questionType)
        {
            
            //quando o tipo escolhido for texto, é desativado a opção para inserção de imagem
            case QuestionType.TEXT:
                questionImg.transform.parent.gameObject.SetActive(false);   
                break;

            case QuestionType.IMAGE:
                //caso o tipo escolhido for imagem, habilita a opção para escolher a imagem 
                questionImg.transform.parent.gameObject.SetActive(true);  
                //quando for tipo imagem, desativa a opção de inserir vídeos  
                questionVideo.transform.gameObject.SetActive(false);       
                //habilita a opção de inserir imagens
                questionImg.transform.gameObject.SetActive(true);  
                //quando for tipo imagem, é desabilitado a opção de inserir audios         
                questionAudio.transform.gameObject.SetActive(false);       
                
                 //define o sprite da imagem na unity
                 questionImg.sprite = question.questionImage;               
                break;

            case QuestionType.AUDIO:
                //caso o tipo escolhido for audio, habilita a opção para escolher vídeos 
                questionVideo.transform.parent.gameObject.SetActive(true);  
                //desabilita a opção de inserir vídeos quando for audio/imagem
                questionVideo.transform.gameObject.SetActive(false);   
                //desabilita a opção para inserir imagens     
                questionImg.transform.gameObject.SetActive(false);       
                //ativa a opção para inserir audios   
                questionAudio.transform.gameObject.SetActive(true);         
                
                //aqui define o áudio a ser inserido
                audioLength = question.audioClip.length;
                //inicia o audio com o video junto                    
                StartCoroutine(PlayAudio());                                
                break;

            case QuestionType.VIDEO:
            //caso o tipo escolhido for vídeo, ativa a opção para inserir imagens
                questionVideo.transform.parent.gameObject.SetActive(true);
                //habilita a opção para inserir a pergunta do vídeo
                questionVideo.transform.gameObject.SetActive(true);   
                //desativa a opção para pergunta de imagens      
                questionImg.transform.gameObject.SetActive(false); 
                //desativa a opção para pergunta de audios 
                questionAudio.transform.gameObject.SetActive(false);        

                //insere o vídeo
                questionVideo.clip = question.videoClip;                    
                //inicia/play o vídeo
                questionVideo.Play();                                      
                break;
        }

        //define o texto da pergunta
        questionInfoText.text = question.questionInfo;                     

        //apresentar a lista de opções das respostas
        List<string> ansOptions = ShuffleList.ShuffleListItems<string>(question.options);

        //atribui opções nos botões de opção
        for (int i = 0; i < options.Count; i++)
        {
            //definindo o texto dentro dos botões para as respostas
            options[i].GetComponentInChildren<Text>().text = ansOptions[i];
            //definindo o nome dos botões
            options[i].name = ansOptions[i];   
            //definindo a cor do botão para padrão normal
            options[i].image.color = normalCol; 
        }

        answered = false;                       

    }

    public void ReduceLife(int remainingLife)
    {
        lifeImageList[remainingLife].color = Color.red;
    }
    
    IEnumerator PlayAudio()
    {
        //caso o tipo da questão escolhida for áudio
        if (question.questionType == QuestionType.AUDIO)
        {
            questionAudio.PlayOneShot(question.audioClip);
            //aguarda alguns segundos
            yield return new WaitForSeconds(audioLength + 0.5f);
            //emite novamente
            StartCoroutine(PlayAudio());
        }
        
        //caso o tipo da questão não for audio
        else 
        {
            //pausa o áudio
            StopCoroutine(PlayAudio());
            //retorna nulo
            yield return null;
        }
    }

    
    //método atribuído aos botões
    void OnClick(Button btn)
    {
        if (quizManager.GameStatus == GameStatus.PLAYING)
        {
            //se a resposta for falsa
            if (!answered)
            {
                //conjunto respondido verdadeiro
                answered = true;
                //pega o valor bool
                bool val = quizManager.Answer(btn.name);
                //caso a resposta for verdadeira
                if (val)
                {
                    //define a cor para corrigir
                    //btn.image.color = correctCol;
                    StartCoroutine(BlinkImg(btn.image));
                }
                else
                {
                    //define a cor para a resposta incorreta
                    btn.image.color = wrongCol;
                }
            }
        }
    }

    
    /// Method to create Category Buttons dynamically
    void CreateCategoryButtons()
    {
        //we loop through all the available catgories in our QuizManager
        for (int i = 0; i < quizManager.QuizData.Count; i++)
        {
            //Create new CategoryBtn
            CategoryBtnScript categoryBtn = Instantiate(categoryBtnPrefab, scrollHolder.transform);
            //Set the button default values
            categoryBtn.SetButton(quizManager.QuizData[i].categoryName, quizManager.QuizData[i].questions.Count);
            int index = i;
            //Add listner to button which calls CategoryBtn method
            categoryBtn.Btn.onClick.AddListener(() => CategoryBtn(index, quizManager.QuizData[index].categoryName));
        }
    }

    //Method called by Category Button
    private void CategoryBtn(int index, string category)
    {
        quizManager.StartGame(index, category); //start the game
        mainMenu.SetActive(false);              //deactivate mainMenu
        gamePanel.SetActive(true);              //activate game panel
    }

    //this give blink effect [if needed use or dont use]
    IEnumerator BlinkImg(Image img)
    {
        for (int i = 0; i < 2; i++)
        {
            img.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            img.color = correctCol;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void RestryButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
