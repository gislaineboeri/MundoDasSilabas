using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth; //autenticador do firebase para registro e login

//script para o autenticador do Firebase
public class FirebaseAuthenticator : MonoBehaviour
{
    public static FirebaseAuthenticator instance; 
    public DependencyStatus dependencyStatus; //status de dependência
    public FirebaseAuth auth; //autenticar
    public FirebaseUser User;

    private void Awake()
    {
        if(instance==null) //checando se existe uma instancia nela
        {
            instance = this;
        }
        else //caso não exista
        {
            Destroy(gameObject);
        }
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => 
        {
            dependencyStatus = task.Result; //a dependência do status recebe o resultado da tarefa
            if(dependencyStatus == DependencyStatus.Available) //checando se está tudo preenchido corretamente
            {
                InitializeFirebase(); //inicia o firebase caso estiver tudo certo
            }
            else //caso estiver faltando algo para iniciar, retorna o erro
            {
                Debug.LogError("Não conseguimos resolver todas as dependências! "+ dependencyStatus);
            }
        });
    }
    void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance; //autenticador recebe a instância base
    }
}
