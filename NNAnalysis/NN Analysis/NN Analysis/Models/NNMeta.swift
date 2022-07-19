//
//  NNMeta.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 2022/07/18.
//

import Foundation

class NNMeta: ObservableObject, Comparable {
    public let fileURL: URL
    public let name: String
    
    public var fitness: Double? = nil
    
    init(fromFile file: URL) throws {
        fileURL = file
        name = file.lastPathComponent
        
        if let jsonData = try? NSData(contentsOfFile: file.path, options: .mappedIfSafe) {
            if let jsonResult: NSDictionary = try? JSONSerialization.jsonObject(with: jsonData as Data, options: JSONSerialization.ReadingOptions.mutableContainers) as? NSDictionary {
               if let fitness : Double = jsonResult["fitness"] as? Double {
                   self.fitness = fitness
               } else {
                   print("Error: meta file missing fitness")
               }
           }
        }
        
        DataHandler.singleton.allMetaFiles.updateValue(self, forKey: file)
    }
    
    static func == (lhs: NNMeta, rhs: NNMeta) -> Bool {
        return lhs.fileURL == rhs.fileURL
    }
    
    static func < (lhs: NNMeta, rhs: NNMeta) -> Bool {
        return (lhs.fitness ?? -1) < (rhs.fitness ?? -1)
    }
}
