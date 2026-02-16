import 'package:flutter/material.dart';
import 'package:mobile/models/Responses/school_dto.dart';

class SchoolDropdown extends StatelessWidget {
  final List<SchoolDto> schools;
  final SchoolDto? selectedSchool;
  final void Function(SchoolDto?) onChanged;
  final String? Function(SchoolDto?)? validator;

  const SchoolDropdown({
    required this.schools,
    required this.selectedSchool,
    required this.onChanged,
    this.validator,
    Key? key,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return DropdownButtonFormField<SchoolDto>(
      decoration: const InputDecoration(
        labelText: 'Kool',
        hintText: 'Vali oma kool',
      ),
      initialValue: selectedSchool,
      items: schools.map((school) {
        return DropdownMenuItem<SchoolDto>(
          value: school,
          child: Text(school.name),
        );
      }).toList(),
      onChanged: onChanged,
      validator: (value) {
        if (validator != null) {
          return validator!(value);
        }
        return null;
      },
    );
  }
}

