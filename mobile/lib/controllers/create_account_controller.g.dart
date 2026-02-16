// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'create_account_controller.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(CreateAccountController)
const createAccountControllerProvider = CreateAccountControllerProvider._();

final class CreateAccountControllerProvider
    extends $NotifierProvider<CreateAccountController, void> {
  const CreateAccountControllerProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'createAccountControllerProvider',
        isAutoDispose: true,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$createAccountControllerHash();

  @$internal
  @override
  CreateAccountController create() => CreateAccountController();

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(void value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<void>(value),
    );
  }
}

String _$createAccountControllerHash() =>
    r'ba9a7dc60b490a80272385c758401ce00a5da8c7';

abstract class _$CreateAccountController extends $Notifier<void> {
  void build();
  @$mustCallSuper
  @override
  void runBuild() {
    build();
    final ref = this.ref as $Ref<void, void>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<void, void>,
              void,
              Object?,
              Object?
            >;
    element.handleValue(ref, null);
  }
}
