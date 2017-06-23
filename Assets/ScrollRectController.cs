using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class ScrollRectController : UIBehaviour, IEndDragHandler, IBeginDragHandler {



	public RectTransform scrollTransformRect; 
	public RectTransform baseRect; // size of each tile content, used for calculations


	public Button backButton;
	public Button nextButton;


	public float duration = 1f;
	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f); // a curve for transitioning in order to give it a little bit of extra polish


	ScrollRect scrollRect; // the scroll rect script

	int targetIndex = 0;
	int itemCount = 0;
	Vector2 baseRectSize;


	public enum eAxis{
		Horizontal,
		Vertical
	}
	public eAxis mAxis = eAxis.Vertical;
		
	public enum eDirection{
		Normal = 1,
		Backwards = -1
	}

	public eDirection mDirection = eDirection.Normal;

	//public List<GameObject> childList;

	protected override void Reset()
	{
		base.Reset();

		if (scrollRect == null) 
			scrollRect = GetComponent<ScrollRect>();
	}


	protected override void Awake(){

		scrollTransformRect.anchoredPosition = GetPositionFromIndex(0);
		itemCount = scrollTransformRect.gameObject.transform.childCount - 1;

		baseRectSize = new Vector2 (baseRect.rect.width, baseRect.rect.height);


		if (backButton != null) {
			
			backButton.onClick.AddListener (delegate {
				OnBackCliked ();
			});
		}


		if (nextButton != null) {

			nextButton.onClick.AddListener (delegate {
				OnNextClicked ();
			});

		}
	}

	public void AddItem(GameObject item){
		item.transform.SetParent(scrollTransformRect.gameObject.transform);

		// reset transform
		item.transform.localScale = new Vector3(1f,1f,1f);
		item.transform.localPosition = new Vector3(0f,0f,0f);

		itemCount = scrollTransformRect.gameObject.transform.childCount - 1;  

	}


	public void Update(){
	}


	// Bts ------

	void OnNextClicked(){
		
		int currentIndex = GetCurrentPositionIndex ();
		int index = currentIndex + 1;

		if (index >= scrollTransformRect.gameObject.transform.childCount) {
			return;
		}

		StopCoroutine (SnapRect ());
		SetPositionFromIndex (index);
	}

	void OnBackCliked(){

		int currentIndex = GetCurrentPositionIndex ();
		int index = currentIndex - 1;

		if (index < 0) {
			return;
		}

		StopCoroutine (SnapRect ());

		SetPositionFromIndex (index);
	}


	// Slider Behaviour ------
	public void OnBeginDrag(PointerEventData eventData)
	{
		StopCoroutine(SnapRect());
	}

	public void OnEndDrag(PointerEventData eventData)
	{      
		targetIndex = GetCurrentPositionIndex();
		StartCoroutine(SnapRect()); 
	}


	// Moves the scrollTransformRect to the desired position
	private IEnumerator SnapRect()
	{
		float timer = 0f; // timer value of course
		while (timer < 1f) // loop until we are done
		{
			Vector2 target = GetPositionFromIndex( targetIndex );
			int axis = (int)mAxis;


			float pos = Mathf.Lerp(scrollTransformRect.anchoredPosition[axis], target[axis], curve.Evaluate(timer));
			timer += 0.1f;

			Vector2 targetVector = new Vector2 ();
			targetVector [axis] = pos;
			scrollTransformRect.anchoredPosition = targetVector; 

			yield return new WaitForEndOfFrame(); // wait until next frame
		}  
	}


	void SetPositionFromIndex(int i, bool animate = true){
		targetIndex = i;
	
		if(!animate){

			var pos = GetPositionFromIndex(i);
			scrollTransformRect.anchoredPosition = pos;
		}else{

			StartCoroutine(SnapRect());                 
		}
	}

	Vector2 GetPositionFromIndex(int i ){

		int axis = (int)mAxis;
		Vector2 v = new Vector2 (0f, 0f);
		v[axis] = baseRectSize[axis] * i * (float)mDirection;


		return v;
	}

	int GetCurrentPositionIndex(){

		float item = GetOffset();

		// backwards
//		item = Mathf.Clamp(-item, 0, itemCount);
		if (mDirection == eDirection.Normal) {
			item = Mathf.Clamp (0, item, itemCount);
		}else if(mDirection == eDirection.Backwards){
			item = Mathf.Clamp (-item, 0, itemCount);
		}


		int v = Mathf.RoundToInt(item);

		return v;
	}

	float GetOffset(){


		int axis = (int)mAxis;
		var anchorPos = scrollTransformRect.anchoredPosition [axis];
	
		float offset = anchorPos / baseRectSize[axis];
		return offset;
	}
		
}
