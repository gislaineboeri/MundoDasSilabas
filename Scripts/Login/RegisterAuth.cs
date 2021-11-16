using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth; //autenticador do firebase
using TMPro; //puxar os input fields do e-mail e senha

public class RegisterAuth : MonoBehaviour
{
    public TMP_InputField usernameRegisterField; //campo para preencher o nome do usuário
    public TMP_InputField emailRegisterField; //campo para preencher o e-mail
    public TMP_InputField passwordRegisterField; //campo para preenher a senha
    public TMP_InputField verifyPasswordRegisterField; //verificar a senha
    public TMP_Text warningRegisterText; //texto caso der algum erro no cadastro

    public void RegisterButton() //função clicar no botão de registro
    {
        StartCoroutine(StartRegister(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text)); //corrotina assim que clicar no botão, registra o email a senha e o nome do usuário
    } 

    private IEnumerator StartRegister(string email, string password, string userName) //registrando os dados
    {
        if(!CheckRegistrationFieldAndReturnForErrors()) //se a verificação retornar algum erro
        {
            var RegisterTask = FirebaseAuthenticator.instance.auth.CreateUserWithEmailAndPasswordAsync(email, password); //puxa do firebase o e-mail e senha do usuário
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted); //aguardar a unity receber algum retorno que está completado

            if(RegisterTask.Exception != null) //se a tarrefa de registro estiver vazia
            {
                HandleRegisterErrors(RegisterTask.Exception);
            }
            else
            {
                StartCoroutine(RegisterUser(RegisterTask, userName, email, password)); //caso contrário, registra o usuário e os dados preenchidos
            }
        }
    }

    bool CheckRegistrationFieldAndReturnForErrors()
    {
        if(usernameRegisterField.text == "") //caso o campo nome do usuário estiver vazio
        {
            warningRegisterText.text = "Nome de usuário vazio"; //retorna a mensagem
            return true;
        }
        else if (passwordRegisterField.text != verifyPasswordRegisterField.text) //caso o campo de senha for diferente do campo verificar senha
        {
            warningRegisterText.text = "Senha e verificar senha não coincidem"; //retorna o aviso
            return true;
        }
        else //caso nenhum dos dois não aconteça, irá verificar falso e vai prosseguir com a função de registrar o usuário
        {
            return false; 
        }
    }

    void HandleRegisterErrors(System.AggregateException registerException)
    {
        //caso o login estiver estiver faltando alguma informação ou for repetida na hora de cadastrar, no console será apresentado o erro
        Debug.LogWarning(message: $"Falha ao registrar a tarefa{registerException}");
        FirebaseException firebaseEx = registerException.GetBaseException() as FirebaseException;
        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

        warningRegisterText.text = DefineRegisterErroMessage(errorCode);
    }

    string DefineRegisterErroMessage(AuthError errorCode)
    {
        switch(errorCode)
        {
            case AuthError.MissingEmail: //caso o usuário não preencher o e-mail, irá retornar o erro abaixo
                return "Preencha o e-mail";
            case AuthError.MissingPassword: //caso o campo da senha ficar em branco, irá retornar o erro abaixo
                return "Preencha a senha";
            case AuthError.WeakPassword: //caso a senha for fraca, irá retornar o erro abaixo
                return "Senha fraca";
            case AuthError.InvalidEmail: //caso o e-mail for inválido, ou seja, não estiver cadastrado no firebase, irá retornar o erro abaixo
                return "E-mail invalido";
            case AuthError.EmailAlreadyInUse: //caso o e-mail já tiver sido cadastrado
                return "Email já registrado";
            default: //caso for uma excessão, irá retornar a mensagem abaixo
                return "Registro falhou, verifique os campos!"; 
        }
    }

    private IEnumerator RegisterUser(System.Threading.Tasks.Task<Firebase.Auth.FirebaseUser> registerTask, string displayName, string email, string password) //registrando o usuário
    {
        FirebaseAuthenticator.instance.User = registerTask.Result;
        //tarefa de perfil para criar um usuário
        if(FirebaseAuthenticator.instance.User != null) //se o autenticador for vazio
        {
            UserProfile profile = new UserProfile {DisplayName = displayName}; //riando um novo usuário, mostrar que o nome de excibição é o mesmo que foi preenchido no cadastro
            var ProfileTask = FirebaseAuthenticator.instance.User.UpdateUserProfileAsync(profile); //checando se houve algum erro na criação deste usuário
            yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted); //aguardando a Unity receber um retorno que está completado
            
            //chegando se tem algum excessão no cadastro ProfileTask
            if(ProfileTask.Exception != null) //caso o perfil for diferente de vazio
            //se for diferente de nulo, significa que temos um erro, passando qual erro teve
            {
                HandleProfileCreationErrors(ProfileTask.Exception); 
                //usuário criado, já está cadastrado no firebase  
            }
            
            else 
            {
                //caso não tiver mais uma excessão, que deu certo para criar o perfil, é trocado a tela de cadastro para a tela de login, e fazer o login do usuário
                ChangeUI.instance.ChangeBetweenLoginAndRegister();
                GetComponent<LoginAuth>().emailInputField.text = email;
                GetComponent<LoginAuth>().passwordInputField.text = password;
                GetComponent<LoginAuth>().LoginButton();
            }
        }
    }

    void HandleProfileCreationErrors(System.AggregateException profileException) //caso for encontrado algum erro, é retornado o tipo do erro utilizando o profileException
    {
        Debug.LogWarning(message: $"Falha no registro {profileException}");
        FirebaseException firebaseEx = profileException.GetBaseException() as FirebaseException;
        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
        warningRegisterText.text = "O nome do usuário falhou, tente novamente!";
    }
}
