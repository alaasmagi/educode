import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:mobile/providers/api_providers.dart';

/// Näide 1: Stream provider kasutamine (automaatne perioodiline kontroll)
/// See kontrollib API staatust iga 30 sekundi tagant
class ApiStatusWidget extends ConsumerWidget {
  const ApiStatusWidget({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final apiStatus = ref.watch(isAppOnlineProvider);

    return apiStatus.when(
      data: (isOnline) {
        return Container(
          padding: const EdgeInsets.all(8),
          decoration: BoxDecoration(
            color: isOnline ? Colors.green : Colors.red,
            borderRadius: BorderRadius.circular(8),
          ),
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(
                isOnline ? Icons.cloud_done : Icons.cloud_off,
                color: Colors.white,
                size: 16,
              ),
              const SizedBox(width: 8),
              Text(
                isOnline ? 'API ühendatud' : 'API ühenduseta',
                style: const TextStyle(color: Colors.white),
              ),
            ],
          ),
        );
      },
      loading: () => const CircularProgressIndicator(),
      error: (error, stack) => const Icon(Icons.error, color: Colors.red),
    );
  }
}

/// Näide 2: Ühekordne kontroll
class OneTimeHealthCheckWidget extends ConsumerWidget {
  const OneTimeHealthCheckWidget({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final apiStatus = ref.watch(isApiOnlineProvider);

    return apiStatus.when(
      data: (isOnline) {
        return Text(isOnline ? 'API töötab' : 'API ei tööta');
      },
      loading: () => const CircularProgressIndicator(),
      error: (error, stack) => Text('Viga: $error'),
    );
  }
}

/// Näide 3: Manuaalne refresh nupuga
class ManualHealthCheckWidget extends ConsumerWidget {
  const ManualHealthCheckWidget({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final healthCheck = ref.watch(healthCheckProvider);

    return Column(
      children: [
        healthCheck.when(
          data: (isOnline) {
            return Card(
              color: isOnline ? Colors.green[50] : Colors.red[50],
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  children: [
                    Icon(
                      isOnline ? Icons.check_circle : Icons.error,
                      color: isOnline ? Colors.green : Colors.red,
                      size: 48,
                    ),
                    const SizedBox(height: 8),
                    Text(
                      isOnline ? 'API on kättesaadav' : 'API ei ole kättesaadav',
                      style: TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.bold,
                        color: isOnline ? Colors.green[900] : Colors.red[900],
                      ),
                    ),
                  ],
                ),
              ),
            );
          },
          loading: () => const Center(child: CircularProgressIndicator()),
          error: (error, stack) => Text('Viga kontrollimisel: $error'),
        ),
        const SizedBox(height: 16),
        ElevatedButton.icon(
          onPressed: () {
            // Refresh provider'it, et teha uus healthcheck
            ref.invalidate(healthCheckProvider);
          },
          icon: const Icon(Icons.refresh),
          label: const Text('Kontrolli uuesti'),
        ),
      ],
    );
  }
}

/// Näide 4: Kasutamine navigatsiooni guardina
class ProtectedRoute extends ConsumerWidget {
  final Widget child;

  const ProtectedRoute({super.key, required this.child});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final apiStatus = ref.watch(isAppOnlineProvider);

    return apiStatus.when(
      data: (isOnline) {
        if (isOnline) {
          return child;
        } else {
          return Scaffold(
            body: Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.cloud_off, size: 64, color: Colors.grey),
                  const SizedBox(height: 16),
                  const Text(
                    'API ei ole kättesaadav',
                    style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 8),
                  const Text('Palun kontrolli oma internetiühendust'),
                  const SizedBox(height: 24),
                  ElevatedButton(
                    onPressed: () {
                      ref.invalidate(isAppOnlineProvider);
                    },
                    child: const Text('Proovi uuesti'),
                  ),
                ],
              ),
            ),
          );
        }
      },
      loading: () => const Scaffold(
        body: Center(child: CircularProgressIndicator()),
      ),
      error: (error, stack) => Scaffold(
        body: Center(child: Text('Viga: $error')),
      ),
    );
  }
}

/// Näide 5: Kasutamine async funktsioonides
class ApiConnectionExample {
  /// Kontrolli API ühendust enne toimingu tegemist
  Future<void> performActionWithHealthCheck(WidgetRef ref) async {
    final generalService = ref.read(generalServiceProvider);

    // Kontrolli API ühendust
    final isOnline = await generalService.healthCheck();

    if (!isOnline) {
      print('API ei ole kättesaadav, ei saa toimingut teha');
      return;
    }

    // Tee API päring
    print('API on online, jätkan toiminguga...');
    // await someService.doSomething();
  }

  /// Kasuta providerit otse
  Future<void> checkStatusFromProvider(WidgetRef ref) async {
    try {
      // Loe FutureProvider väärtust
      final isOnline = await ref.read(isApiOnlineProvider.future);

      if (isOnline) {
        print('✓ API on kättesaadav');
      } else {
        print('✗ API ei ole kättesaadav');
      }
    } catch (e) {
      print('Viga API kontrollimisel: $e');
    }
  }
}

/// Näide 6: SnackBar teavitus
class ApiStatusSnackBar extends ConsumerStatefulWidget {
  final Widget child;

  const ApiStatusSnackBar({super.key, required this.child});

  @override
  ConsumerState<ApiStatusSnackBar> createState() => _ApiStatusSnackBarState();
}

class _ApiStatusSnackBarState extends ConsumerState<ApiStatusSnackBar> {
  bool? _previousStatus;

  @override
  Widget build(BuildContext context) {
    final apiStatus = ref.watch(isAppOnlineProvider);

    ref.listen(isAppOnlineProvider, (previous, next) {
      next.whenData((isOnline) {
        // Näita snackbar'i ainult kui staatus muutub
        if (_previousStatus != null && _previousStatus != isOnline) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Row(
                children: [
                  Icon(
                    isOnline ? Icons.cloud_done : Icons.cloud_off,
                    color: Colors.white,
                  ),
                  const SizedBox(width: 8),
                  Text(isOnline ? 'API ühendus taastatud' : 'API ühendus katkes'),
                ],
              ),
              backgroundColor: isOnline ? Colors.green : Colors.red,
              duration: const Duration(seconds: 3),
            ),
          );
        }
        _previousStatus = isOnline;
      });
    });

    return widget.child;
  }
}

/// Näide 7: App baari integratsioon
class MyAppBar extends ConsumerWidget implements PreferredSizeWidget {
  final String title;

  const MyAppBar({super.key, required this.title});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final apiStatus = ref.watch(isAppOnlineProvider);

    return AppBar(
      title: Text(title),
      actions: [
        apiStatus.when(
          data: (isOnline) => IconButton(
            icon: Icon(
              isOnline ? Icons.cloud_done : Icons.cloud_off,
              color: isOnline ? Colors.green : Colors.red,
            ),
            tooltip: isOnline ? 'API ühendatud' : 'API ühenduseta',
            onPressed: () {
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  content: Text(
                    isOnline ? 'API töötab korralikult' : 'API ei ole kättesaadav',
                  ),
                ),
              );
            },
          ),
          loading: () => const Padding(
            padding: EdgeInsets.all(16.0),
            child: SizedBox(
              width: 20,
              height: 20,
              child: CircularProgressIndicator(strokeWidth: 2),
            ),
          ),
          error: (_, __) => const Icon(Icons.error, color: Colors.red),
        ),
      ],
    );
  }

  @override
  Size get preferredSize => const Size.fromHeight(kToolbarHeight);
}

/// Täielik näide screen'is
class HealthCheckExampleScreen extends ConsumerWidget {
  const HealthCheckExampleScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return Scaffold(
      appBar: const MyAppBar(title: 'API Health Check'),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            const Text(
              'API Staatus',
              style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 16),

            // Automaatne perioodiline kontroll
            const ApiStatusWidget(),
            const SizedBox(height: 24),

            // Manuaalne kontroll
            const ManualHealthCheckWidget(),
            const SizedBox(height: 24),

            const Divider(),
            const SizedBox(height: 16),

            // Info tekst
            const Text(
              'Automaatne kontroll käib iga 30 sekundi tagant',
              style: TextStyle(color: Colors.grey),
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
    );
  }
}

