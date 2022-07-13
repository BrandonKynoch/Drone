//
//  ValidationFields.swift
//  NN Analysis
//
//  Created by Brandon Kynoch on 2022/07/10.
//

import Foundation

class ValidationFields: ObservableObject {
    @Published public var fields = [Entry]()
    
    public func GetField(withName: String) -> Entry? {
        for field in fields {
            if field.name == withName {
                return field
            }
        }
        return nil
    }
    
    func AddFunction(parameterName: String, function: @escaping () -> (Bool)) {
        self.fields.append(Entry(forField: parameterName, validationFunction: function))
    }
    
    func ValidateAll() {
        for field in fields {
            field.Validate()
        }
    }
    
    func AllFieldsAreValid() -> Bool {
        for field in fields {
            if !field.valid {
                return false
            }
        }
        return true
    }
    
    func SetAllFieldsValid() {
        for field in fields {
            field.valid = true
        }
    }
    
    class Entry: ObservableObject {
        public var name: String = ""
        @Published public var valid: Bool = true
        
        public let validationFunction: () -> (Bool)
        
        public static let nilValidationField = Entry(forField: "", validationFunction: { return true })
        
        init (forField: String, validationFunction: @escaping () -> (Bool)) {
            self.name = forField
            self.valid = true
            self.validationFunction = validationFunction
        }
        
        func Validate() {
            valid = validationFunction()
        }
    }
}
