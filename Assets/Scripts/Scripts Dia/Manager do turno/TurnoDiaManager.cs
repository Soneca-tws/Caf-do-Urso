using System.Collections.Generic;
using UnityEngine;

public class TurnoDiaManager : MonoBehaviour
{
    [Header("Fila de Clientes da Cena")]
    [Tooltip("Arraste os clientes da Hierarchy (Malandro Chamado Jorge, Rita Cadillac) na ordem de entrada.")]
    [SerializeField] private List<ClienteMovimento> filaDeClientes = new List<ClienteMovimento>();

    [Header("Configuração de Encontro")]
    [Tooltip("Índice do diálogo na lista de cada cliente (0 = 1º encontro, 1 = 2º encontro).")]
    [SerializeField] private int indiceEncontroAtual = 0;
    [SerializeField] private float intervaloEntreClientes = 1.0f;

    private int indiceClienteAtual = 0;

    private void Start()
    {
        for (int i = 0; i < filaDeClientes.Count; i++)
        {
            if (filaDeClientes[i] != null)
            {
                filaDeClientes[i].gameObject.SetActive(false);
            }
        }

        ChamarProximoCliente();
    }

    private void ChamarProximoCliente()
    {
        if (indiceClienteAtual >= filaDeClientes.Count)
        {
            if (FadeManager.Instancia != null)
            {
                FadeManager.Instancia.TransicaoParaNoite();
            }
            return;
        }

        ClienteMovimento clienteAtual = filaDeClientes[indiceClienteAtual];
        if (clienteAtual == null)
        {
            indiceClienteAtual++;
            ChamarProximoCliente();
            return;
        }

        DialogueTrigger trigger = clienteAtual.GetComponent<DialogueTrigger>();
        if (trigger != null)
        {
            trigger.DefinirDialogoAtual(indiceEncontroAtual);
        }

        clienteAtual.OnClienteSaiu += AoClienteSair;
        clienteAtual.gameObject.SetActive(true);
    }

    private void AoClienteSair(ClienteMovimento cliente)
    {
        cliente.OnClienteSaiu -= AoClienteSair;
        cliente.gameObject.SetActive(false);

        if (DialogueView.Instancia != null)
        {
            DialogueView.Instancia.ForcarFechamento();
        }

        indiceClienteAtual++;
        Invoke(nameof(ChamarProximoCliente), intervaloEntreClientes);
    }
}