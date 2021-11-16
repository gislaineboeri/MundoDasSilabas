using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth; //autenticador do firebase
using Firebase;
using TMPro; //puxar os input fields do e-mail e senha
using UnityEngine.SceneManagement;

public class LoginAuth : MonoBehaviour
{
    public TMP_InputField emailInputField; //entrada para preencher o e-mail
    public TMP_InputField passwordInputField; //entrada para preencher a senha 
    public TMP_Text warningLoginText; //texto de erro para o login caso estiver incorreto qualquer informação
    public TMP_Text confirmLoginText; //texto de sucesso, informar que deu tudo certo

    public void LoginButton() //função clicando no botão de login
    {
        StartCoroutine(StartLogin(emailInputField.text, passwordInputField.text)); //inicia a corrotina do login e senha caso estiver preenchido corretamente e não retornar nenhum erro no console 
    }

    private IEnumerator StartLogin(string email, string password) //recebe duas strings, o e-mail e a senha
    {
        var LoginTask = FirebaseAuthenticator.instance.auth.SignInWithEmailAndPasswordAsync(email, password); //recebe a instância do firebase autenticador e chama a função de e-mail e senha
        yield return new WaitUntil(predicate: ()=> LoginTask.IsCompleted); //faz com que a Unity aguarde a resposta do identifcador do Firebase, chamando a função com o e-mail e senha

        if(LoginTask.Exception != null) //caso o login for diferente de nulo
        {
            HandleLoginErrors(LoginTask.Exception); //
        }
        else //caso não teve nenhuma excessão
        {
            LoginUser(LoginTask); //usuário logado
        }
    }

    void HandleLoginErrors(System.AggregateException loginException) //lindando com os erros de login
    {
        Debug.LogWarning(message: $"Falha na tarefa de login{loginException}"); //retorna a mensagem informando que está com alguma falha na tarefa do login
        //caso o login estiver estiver faltando alguma informação ou for repetida, no console será apresentado o erro
        FirebaseException firebaseEx = loginException.GetBaseException() as FirebaseException; 
        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

        warningLoginText.text = DefineLoginErrorMessage(errorCode); //apresenta qual é o erro no login
    }

    string DefineLoginErrorMessage(AuthError errorCode)
    {
        switch(errorCode)
        {
            case AuthError.MissingEmail: //caso o usuário não preencher o e-mail, irá retornar o erro abaixo
                return "Preencha o e-mail";
            case AuthError.MissingPassword: //caso o campo da senha ficar em branco, irá retornar o erro abaixo
                return "Preencha a senha";
            case AuthError.InvalidEmail: //caso o e-mail for inválido, ou seja, não estiver cadastrado no firebase, irá retornar o erro abaixo
                return "E-mail inválido";
            case AuthError.UserNotFound: //caso a autenticação não der certo com o e-mail e senha, irá retornar que a conta não existe
                return "A conta não existe";
            default: //caso contrário, senão for nenhum desses erros, irá retornar o erro abaixo
                return "Falha no login, verifique os campos preenchidos!";
        }
    }

    void LoginUser(System.Threading.Tasks.Task<Firebase.Auth.FirebaseUser> loginTask) //criando a função do usuário de login
    {
        FirebaseAuthenticator.instance.User = loginTask.Result; //no firebase acessa a instância e acessa o usuário que está cadastrado nessa intância, passando o resultado da tarefa de login
        Debug.LogFormat("Usuário logado com sucesso: {0} ({1})", FirebaseAuthenticator.instance.User.DisplayName, FirebaseAuthenticator.instance.User.Email); //retorna a mensagem que logou com sucesso quando a tarefa de login for bem sucedida

        warningLoginText.text = ""; //vazio para quando não estiver preenchido os campos de login

        if(FirebaseAuthenticator.instance.User.DisplayName != null){ //caso estiver tudo certo com o login, ele passa para a cena do joog, caso contrário, retorna o erro
            SceneManager.LoadScene("MenuOpcoes");
        }
        else
        {
            confirmLoginText.text = "Acesso incorreto";
        }
        
    }
   
}