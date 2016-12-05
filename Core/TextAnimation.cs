using UnityEngine;
using System.Collections;
using TMPro;
using System;

public class TextAnimation : MonoBehaviour
{
	public Transform AttachTransform { get; private set; }

	public float PushTime = 0.1f;
	public float PushPosition = 1f;
	public float PushPositionCritical = 1f;
	public float PushPositionDot = 1f;
	public float PushPositionCriticalDot = 1f;
	public float Depth = -1f;
	public bool IsPlaying { get; private set; }
	public float PlaybackTime { get; set; }

	public eTextPushType PushType { get; private set; }

	public float AddPosition { get; set; }

	[Serializable]
	public class AnimationUnit
	{
		public float Delay = 0f;
		public float Duration = 1f;
		public float Start = 0f;
		public float End = 0f;

		public float Length { get { return Delay + Duration; } }
		public AnimationCurve Curve = new AnimationCurve();

		public float Evaluate(float playback_time)
		{
			if (playback_time < Delay)
				return Start;

			float local_time = (playback_time - Delay) / Duration;
			return Start + Curve.Evaluate(local_time) * (End - Start);
		}
	}

	public interface IText
	{
		Color color { get; set; }
		string text { get; set; }
		int fontSize { get; set; }
	}

	class Text_TextMeshPro : IText
	{
		public Text_TextMeshPro(TextMeshPro text)
		{
			this.Text = text;
			Text.sortingOrder = 3;
		}
		TextMeshPro Text;
		public Color color { get { return Text.color; } set { Text.color = value; } }
		public string text { get { return Text.text; } set { Text.text = value; } }
		public int fontSize { get { return (int)Text.fontSize; } set { Text.fontSize = value; } }
	}

	class Text_UGUI_Text : IText
	{
		public Text_UGUI_Text(UnityEngine.UI.Text text)
		{
			this.Text = text;
		}
		UnityEngine.UI.Text Text;
		public Color color { get { return Text.color; } set { Text.color = value; } }
		public string text { get { return Text.text; } set { Text.text = value; } }
		public int fontSize { get { return Text.fontSize; } set { Text.fontSize = Mathf.RoundToInt(value * 1.3f); } }
	}

	public bool IsUGUI { get { return m_Text is Text_UGUI_Text; } }

	IText m_Text;
	float m_Scale = 1f;

	public IText Text { get { return m_Text; } }

	public AnimationUnit Position, Scale, Alpha;

	public float CurrentPosition
	{
		get
		{
			return Position.Evaluate(PlaybackTime) + AddPosition;
		}
	}

	void CheckText()
	{
		if (m_Text != null)
			return;

		var tmp = GetComponent<TextMeshPro>();
		if (tmp != null)
			m_Text = new Text_TextMeshPro(tmp);
		else
			m_Text = new Text_UGUI_Text(GetComponent<UnityEngine.UI.Text>());
	}

	void Reset()
	{
		transform.position = new Vector3(0f, Position.Start - Depth*0.15f, Depth) + AttachTransform.position;

		Vector3 scale = Vector3.one * Scale.Start;
		scale.z = 1f;
		transform.localScale = (IsUGUI ? 0.05f : 1f) * scale;

		Color32 color = m_Text.color;
		color.a = (byte)(255 * Alpha.Start);
		m_Text.color = color;

		PlaybackTime = 0f;
		AddPosition = 0f;
		IsPlaying = true;
	}

	// Update is called once per frame
	void Update ()
	{
		if (IsPlaying == false)
			return;

		PlaybackTime += Time.deltaTime;

		Sample();
	}

	public void Sample()
	{
		if (IsPlaying == false)
			return;

		transform.position = new Vector3(0f, CurrentPosition * m_Scale - Depth * 0.15f, Depth + PlaybackTime * 0.01f) + AttachTransform.position;

		Vector3 scale = Vector3.one * Scale.Evaluate(PlaybackTime);
		scale.z = 1f;
		transform.localScale = (IsUGUI ? 0.05f : 1f) * scale;

		Color32 color = m_Text.color;
		color.a = (byte)(255 * Alpha.Evaluate(PlaybackTime));
		m_Text.color = color;

		IsPlaying = PlaybackTime < Position.Length || PlaybackTime < Scale.Length || PlaybackTime < Alpha.Length;

		if (IsPlaying == false)
		{
			AttachTransform = null;
		}
	}

	public void Init(Transform attach_transform, float text_offset, string text, eTextPushType push_type, float scale, float add_position)
	{
		CheckText();

		AttachTransform = attach_transform;
		PushType = push_type;
		m_Text.text = text;
		m_Scale = scale;
		Reset();

		Depth = text_offset;

		AddPosition = 2f + add_position;
		switch(push_type)
		{
			case eTextPushType.Critical:
				m_Text.fontSize = 22;
				AddPosition += 1f;
				break;

			case eTextPushType.Normal:
				m_Text.fontSize = 20;
				break;

			case eTextPushType.Dot:
				m_Text.fontSize = 16;
				break;

			case eTextPushType.CriticalDot:
				m_Text.fontSize = 18;
				break;
		}
	}
}
