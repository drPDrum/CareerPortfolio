using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectS
{
    public enum ESceneType
    {
        None = -1,
        Logo,
        Loading,
        Title,

        Shelter,
        Attack,
        Defense,
        Explore,
        Disaster,
        WorldMap,

        Visit,

        Cinema,
        BattleMode, //테스트 개발 씬
    }

    /// <summary>
    /// 인게임에서 로드하는 씬에 메인 컨트롤러
    /// 이 스크립트를 상속받아야 SceneManager로 관리되며, 씬 로드 및 배치, 가비지 정리, 이벤트 처리 등을 담당.
    /// SceneManager::LoadScene(..) -> UIManager::ShowLoading(..) -> 기존 Scene 정리(FX, SFX정리, SceneController::OnEndScene(..)
    /// -> 실제 Scene Load처리 -> SceneController::Initialize() -> SceneManager::OnEndLoading(..) -> UIManager::OutLoading(..) -> SceneController::OnStartScene()
    /// </summary>
    public class SceneControllerBase : MonoBehaviour
    {
        public ESceneType SceneType { get; protected set; } = ESceneType.None;


        protected virtual void Start()
        {
            StartCoroutine(Initialize());
        }

        /// <summary>
        /// GC.Collect 호출과 Scene Manager에게 로딩이 끝남을 알려줍니다.
        /// 이 함수를 호출하지 않으면, OnStartScene이 호출되지 않는다.
        /// 이 함수 overriding해서 씬 상태를 넣을 것.
        /// </summary>
        protected virtual IEnumerator Initialize()
        {
            yield return null;
            
            System.GC.Collect();

            // 이 호출스택에서 로딩 UI를 종료하고 this.OnStartScene를 호출합니다.
            Managers.Scene.OnEndLoading(this);
        }

        /// <summary>
        /// 씬 시작 이벤트
        /// </summary>
        public virtual void OnStartScene() { }

        /// <summary>
        /// 씬 종료 이벤트
        /// 씬 전환 호출 직전에 호출되어 OnDestroy 보다 빠름
        /// </summary>
        public virtual void OnEndScene() 
        {
        }
    }
}
