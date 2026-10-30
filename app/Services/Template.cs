// namespace App.Template;

// public abstract class TemplateEventArgs<T>(T value) : EventArgs
// {
// 	protected virtual T? Value { get; } = value;
// }

// public abstract class Template<T>
// {
// 	public event EventHandler<TemplateEventArgs<T>>? UseTemplateEvent;

// 	protected virtual void TouchClick(T value) { }
// 	protected virtual void OnUseTemplateEvent(TemplateEventArgs<T> e) { }
// }

// public abstract class TemplateController<T>
// {
// 	readonly Template<T>? template;

// 	protected TemplateController(Template<T> template)
// 	{
// 		this.template = template;
// 		this.template.UseTemplateEvent += OnUseTemplateEvent;
// 	}

// 	protected virtual void OnUseTemplateEvent(object? sender, TemplateEventArgs<T> e) { }

// 	protected virtual void CancelUseTemplateEvent() => template?.UseTemplateEvent -= OnUseTemplateEvent;
// }

