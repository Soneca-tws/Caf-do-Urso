using UnityEngine;

public class BillboardY : MonoBehaviour
{
    public enum ModoOrientacao
    {
        FixoParaFrente,
        ParaleloACamera
    }

    [Header("Configuração")]
    [SerializeField] private ModoOrientacao modo = ModoOrientacao.FixoParaFrente;

    [Tooltip("Usado no modo FixoParaFrente (0 ou 180 dependendo da frente do seu cenário).")]
    [SerializeField] private float anguloYFixo = 0f;

    private Transform camTransform;

    private void Start()
    {
        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (modo == ModoOrientacao.FixoParaFrente)
        {
            transform.rotation = Quaternion.Euler(0f, anguloYFixo, 0f);
            return;
        }

        if (camTransform == null)
        {
            if (Camera.main != null) camTransform = Camera.main.transform;
            else return;
        }

        Vector3 frenteCamera = camTransform.forward;
        frenteCamera.y = 0f;

        if (frenteCamera.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(frenteCamera);
        }
    }
}