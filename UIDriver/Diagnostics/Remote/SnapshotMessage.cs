using System.Text.Json.Serialization;

namespace UIDriver.Diagnostics.Remote;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(TreeSnapshotMessage), "tree")]
[JsonDerivedType(typeof(BranchSnapshotMessage), "branch")]
public abstract record SnapshotMessage;
