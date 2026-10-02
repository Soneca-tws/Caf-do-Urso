using UnityEngine;
using UnityEngine.InputSystem;

public class SlowMotionManager : MonoBehaviour
{
    [Header("Configurações de Tempo")]
    [Tooltip("O quão lento o jogo vai ficar. 0.2 = 20% da velocidade normal.")]
    public float slowTimeScale = 0.2f; 
    
    [Tooltip("Duração do poder em segundos (tempo real).")]
    public float duration = 3f; 
    
    private bool isSlowMo = false;
    private float timer;
    private float defaultFixedDeltaTime;

    private void Start()
    {
        // Salva o valor original da física da Unity (geralmente 0.02)
        defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void Update()
    {
        // Verifica se apertou F e se já não está em câmera lenta
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame && !isSlowMo)
        {
            StartSlowMotion();
        }

        // Controla o tempo para voltar ao normal
        if (isSlowMo)
        {
            // IMPORTANTE: Usamos unscaledDeltaTime porque o deltaTime normal está em câmera lenta!
            timer -= Time.unscaledDeltaTime;
            
            if (timer <= 0)
            {
                StopSlowMotion();
            }
        }
    }

    private void StartSlowMotion()
    {
        isSlowMo = true;
        timer = duration;
        
        // Deixa o jogo em câmera lenta
        Time.timeScale = slowTimeScale;
        
        // Ajusta a atualização da física para o jogo não ficar "gaguejando" (travando) no slow motion
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
        
        Debug.Log("Câmera Lenta ATIVADA!");
    }

    private void StopSlowMotion()
    {
        isSlowMo = false;
        
        // Volta o tempo ao normal
        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
        
        Debug.Log("Tempo NORMAL!");
    }
}