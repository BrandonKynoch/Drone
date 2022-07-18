//
//  SwiftExtra.swift
//  Curator
//
//  Created by Brandon Kynoch on 2021/03/31.
//

import Foundation
import SwiftUI
//import MapKit

class SwiftExtras {
    private static var privateSingleton: SwiftExtras?
    public static var singleton: SwiftExtras = {
        if privateSingleton == nil {
            privateSingleton = SwiftExtras()
        }
        return privateSingleton!
    }()
    
    
    public var dateFormatter: DateFormatter {
        let formatter = DateFormatter()
        formatter.dateFormat = "dd MMMM"
        formatter.locale = Locale(identifier: "en_US")
        return formatter
    }
    
    init() {
        guard SwiftExtras.privateSingleton == nil else {
            return
        }
        SwiftExtras.privateSingleton = self
    }
}

extension Notification.Name {
    // MARK: Custom notifications
    public static let OnApplicationTerminate = Notification.Name("OnApplicationTerminate")
    
    public static let OnTabChanged = Notification.Name("OnTabChanged")
    
    public static let OnSearchBarOpen = Notification.Name("OnSearchBarOpen")
    public static let OnSearchCancel = Notification.Name("OnSearchCancel")
    
    public static let OnBeginEdit = Notification.Name("OnBeginEdit")
    public static let OnEndEdit = Notification.Name("OnEndEdit")
    
    public static let WillCreateNewRecord = Notification.Name("WillCreateNewRecord")
}

protocol NumericType: Equatable, Comparable {
    static func +(lhs: Self, rhs: Self) -> Self
    static func -(lhs: Self, rhs: Self) -> Self
    static func *(lhs: Self, rhs: Self) -> Self
    static func /(lhs: Self, rhs: Self) -> Self
    //static func %(lhs: Self, rhs: Self) -> Self
    init(_ v: Int)
}

//extension Double: NumericType {}
//extension CGFloat: NumericType {}
//extension Int: NumericType {}

public func lerp(_ a: CGFloat, _ b: CGFloat, t: CGFloat) -> CGFloat {
    let tC = t.clamped(0...1)
    return (a * (1 - tC)) + (b * tC)
}



public func getAllFilesInDirectory(bundleDirectory: FileManager.SearchPathDirectory, directory: String, extensionWanted: String) -> (names: [String], paths: [URL]) {
    let cachesURL = FileManager.default.urls(for: bundleDirectory, in: .userDomainMask).first!
    let directoryURL = cachesURL.appendingPathComponent(directory).absoluteURL.standardizedFileURL

    do {
        try FileManager.default.createDirectory(atPath: directoryURL.relativePath, withIntermediateDirectories: true)
        
        // Get the directory contents urls (including subfolders urls)
        let directoryContents = try FileManager.default.contentsOfDirectory(at: directoryURL, includingPropertiesForKeys: nil, options: [])

        // Filter the directory contents
        let filesPath = directoryContents.filter{ $0.pathExtension == extensionWanted }
        let fileNames = filesPath.map{ $0.deletingPathExtension().lastPathComponent }

        return (names: fileNames, paths: filesPath);

    } catch {
        print("Failed to fetch contents of directory: \(error.localizedDescription)")
    }

    return (names: [], paths: [])
}


public func getAllFilesInDirectory(directory: URL, extensionWanted: String?) -> (names: [String], paths: [URL]) {
    do {
        directory.startAccessingSecurityScopedResource()
        
        // Get the directory contents urls (including subfolders urls)
        let directoryContents = try FileManager.default.contentsOfDirectory(at: directory, includingPropertiesForKeys: nil, options: [])
        
        var filesPath = directoryContents
        
        if let extensionWanted = extensionWanted {
            // Filter the directory contents
            filesPath = directoryContents.filter{ $0.pathExtension == extensionWanted }
        }
        
        let fileNames = filesPath.map{ $0.deletingPathExtension().lastPathComponent }

        directory.stopAccessingSecurityScopedResource()
        
        return (names: fileNames, paths: filesPath);

    } catch {
        print("Failed to fetch contents of directory: \(error.localizedDescription)")
    }

    return (names: [], paths: [])
}




public func urlRemovingBundleSubDirectory(bundleDirectory: FileManager.SearchPathDirectory, _ url: URL) -> URL {
    var path = url.path
    path = path.deletingPrefix(FileManager.default.urls(for: bundleDirectory, in: .userDomainMask).first!.path)
    return URL(fileURLWithPath: path)
}

public func urlAppendingBundleSubDirectory(bundleDirectory: FileManager.SearchPathDirectory, _ url: URL) -> URL {
    var path = url.path
    path = "\(FileManager.default.urls(for: bundleDirectory, in: .userDomainMask).first!.path)\(url.path)"
    return URL(fileURLWithPath: path)
}


extension String {
    func deletingPrefix(_ prefix: String) -> String {
        guard self.hasPrefix(prefix) else { return self }
        return String(self.dropFirst(prefix.count))
    }
}




extension NSColor {
    public convenience init?(hex: String) {
        let r, g, b: CGFloat
        let hexN = "\(hex)ff"

        if hexN.hasPrefix("#") {
            let start = hexN.index(hexN.startIndex, offsetBy: 1)
            let hexColor = String(hexN[start...])

            if hexColor.count == 8 {
                let scanner = Scanner(string: hexColor)
                var hexNumber: UInt64 = 0

                if scanner.scanHexInt64(&hexNumber) {
                    r = CGFloat((hexNumber & 0xff000000) >> 24) / 255
                    g = CGFloat((hexNumber & 0x00ff0000) >> 16) / 255
                    b = CGFloat((hexNumber & 0x0000ff00) >> 8) / 255

                    self.init(red: r, green: g, blue: b, alpha: 1)
                    return
                }
            }
        }

        return nil
    }
}


extension Comparable {
    func clamped(_ limits: ClosedRange<Self>) -> Self {
        return min(max(self, limits.lowerBound), limits.upperBound)
    }
}



extension  StringProtocol {
    var removingPunctuation: [SubSequence] {
        split(whereSeparator: \.isLetter.negation)
    }
}



extension Bool {
    var negation: Bool { !self }
}









struct IndexedCollection<Base: RandomAccessCollection>: RandomAccessCollection {
    typealias Index = Base.Index
    typealias Element = (index: Index, element: Base.Element)

    let base: Base

    var startIndex: Index { base.startIndex }

    var endIndex: Index { base.endIndex }

    func index(after i: Index) -> Index {
        base.index(after: i)
    }

    func index(before i: Index) -> Index {
        base.index(before: i)
    }

    func index(_ i: Index, offsetBy distance: Int) -> Index {
        base.index(i, offsetBy: distance)
    }

    subscript(position: Index) -> Element {
        (index: position, element: base[position])
    }
}

extension RandomAccessCollection {
    func indexed() -> IndexedCollection<Self> {
        IndexedCollection(base: self)
    }
}



extension UserDefaults {
    open func setStruct<T: Codable>(_ value: T?, forKey defaultName: String){
        let data = try? JSONEncoder().encode(value)
        set(data, forKey: defaultName)
    }
    
    open func structData<T>(_ type: T.Type, forKey defaultName: String) -> T? where T : Decodable {
        guard let encodedData = data(forKey: defaultName) else {
            return nil
        }
        
        return try! JSONDecoder().decode(type, from: encodedData)
    }
    
    open func setStructArray<T: Codable>(_ value: [T], forKey defaultName: String){
        let data = value.map { try? JSONEncoder().encode($0) }
        
        set(data, forKey: defaultName)
    }
    
    open func structArrayData<T>(_ type: T.Type, forKey defaultName: String) -> [T] where T : Decodable {
        guard let encodedData = array(forKey: defaultName) as? [Data] else {
            return []
        }
        
        return encodedData.map { try! JSONDecoder().decode(type, from: $0) }
    }
}


extension Bool {
    init(_ fromString: String) {
        self.init(fromString == "Optional(1)" || fromString == "1")
    }
}



//class MapLocation: Identifiable, ObservableObject, Equatable {
//    let id = UUID()
//    @Published var address: String? // full address
//    var street: String? = nil
//    var city: String? = nil
//    @Published var addressState: String?
//    @Published var code: String?
//    @Published var coordinate: CLLocationCoordinate2D
//    @Published var viewingRegion: MKCoordinateRegion
//    @Published var isSet: Bool
//
//    init(address: String, coordinate: CLLocationCoordinate2D) {
//        self.address = address
//        self.coordinate = coordinate
//        self.isSet = true
//        self.viewingRegion = MKCoordinateRegion(
//            center: coordinate,
//            span: MKCoordinateSpan(latitudeDelta: 0.05, longitudeDelta: 0.05))
//        self.code = nil
//    }
//
//    init() {
//        self.address = nil
//        self.coordinate = CLLocationCoordinate2D()
//        self.isSet = false
//        self.viewingRegion = MKCoordinateRegion()
//        self.code = nil
//    }
//
//    func setAddress(mapItem: MKMapItem) {
//        let streetNum = mapItem.placemark.subThoroughfare != nil ? "\(mapItem.placemark.subThoroughfare!) " : ""
//        let streetName = mapItem.placemark.thoroughfare != nil ? "\(mapItem.placemark.thoroughfare!)" : ""
//
//        self.street = "\(streetNum)\(streetName)"
//        self.city = mapItem.placemark.locality != nil ? "\(mapItem.placemark.locality!)" : ""
//        self.addressState = mapItem.placemark.administrativeArea
//
//        self.code = mapItem.placemark.postalCode
//
//        let formattedStreetName = mapItem.placemark.thoroughfare != nil ? "\(mapItem.placemark.thoroughfare!), " : ""
//        let formattedCity = mapItem.placemark.locality != nil ? "\(mapItem.placemark.locality!), " : ""
//
//        self.address = "\(streetNum)\(formattedStreetName)\(formattedCity)\(self.addressState ?? "")"
//        self.coordinate = mapItem.placemark.coordinate
//        self.isSet = true
//
//        resetViewingRegion()
//
//        self.objectWillChange.send()
//    }
//
//    func setStreet(_ street: String) {
//        if street != "??" {
//            self.street = street
//        }
//    }
//
//    func setCity(_ city: String) {
//        if city != "??" {
//            self.city = city
//        }
//    }
//
//    func setState(_ state: String) {
//        if state != "??" {
//            self.addressState = state
//        }
//    }
//
//    func updateAddressString() {
//        let formattedStreetName = (street != nil && street != "" && street != "??") ? "\(street!), " : ""
//        let formattedCity = (city != nil && city != "" && city != "??") ? "\(city!), " : ""
//        let formattedState = (addressState != nil && addressState != "" && addressState != "??") ? addressState! : ""
//
//        self.address = "\(formattedStreetName)\(formattedCity)\(formattedState)"
//    }
//
//    func resetViewingRegion(){
//        if isSet {
//            self.viewingRegion = MKCoordinateRegion(
//                center: coordinate,
//                span: MKCoordinateSpan(latitudeDelta: 0.05, longitudeDelta: 0.05))
//        } else {
//            self.viewingRegion = MKCoordinateRegion()
//        }
//    }
//
//    static func == (lhs: MapLocation, rhs: MapLocation) -> Bool {
//        return lhs.coordinate.latitude == rhs.coordinate.latitude && lhs.coordinate.longitude == rhs.coordinate.longitude
//    }
//}




extension URL {
    var isDirectory: Bool? {
        do {
            return (try resourceValues(forKeys: [URLResourceKey.isDirectoryKey]).isDirectory)
        }
        catch let error {
            print(error.localizedDescription)
            return nil
        }
    }
}


extension UnsafeRawBufferPointer {
    public func ReadBytes<T>(_ type: T.Type, offset: inout Int) -> T {
        let returnVal = self.load(fromByteOffset: offset, as: type)
        offset += MemoryLayout<T>.size
        return returnVal
    }
}






public class Trie<T> {
    var root: Node<T>
    
    init() {
        self.root = Node(charKey: nil)
    }
    
    // insert string
    internal func insert(_ key: String, _ val: T) {
        guard !key.isEmpty else { return }
        
        var node = self.root
        for (index, char) in key.lowercased().enumerated() {
            if let child = node.children[char] {
                node = child
            } else {
                let newNode = Node<T>(charKey: char)
                newNode.parent = node
                node.children.updateValue(newNode, forKey: char)
                node = newNode
            }
            
            if index == key.count-1 {
                node.isEnd = true
                node.value = val
            }
        }
    }
    
    // check if it contains a string
    internal func contains(key: String) -> Bool {
        guard !key.isEmpty else { return false }
        
        var node = self.root
        for char in key {
            if let child = node.children[char] {
                node = child
                
                if char == node.charKey && node.isEnd {
                    return true                                     // Check this - Check that it is end of inputkey
                }
            }
        }
        
        return false
    }
    
    // check if given prefix exists
    internal func contains(prefix: String) -> Bool {
        guard !prefix.isEmpty else { return false }
        
        var node = self.root
        for (index, char) in prefix.enumerated() {
            if let child = node.children[char] {
                node = child
                
                // if it's at the end of the count
                if char == node.charKey && index == prefix.count-1 {
                    return true
                }
            }
        }
        
        return false
    }
    
    // find all words with given prefix
    internal func find(prefix: String) -> [(key: String, value: T)] {
        guard !prefix.isEmpty else { return [] }
        var output: [(String, T)] = []
        
        var node = self.root
        for (index, char) in prefix.enumerated() {
            if let child = node.children[char] {
                node = child
                if char == node.charKey && index == prefix.count-1 {
                    self.getWords(node: node, arr: &output)
                }
            }
        }
        
        return output
    }
    
    private func getWords(node: Node<T>, arr: inout [(String, T)]) {
        if node.isEnd {
            arr.append(self.getWord(node: node))
        }
        
        for child in node.children {
            self.getWords(node: child.value, arr: &arr)
        }
    }
    
    private func getWord(node: Node<T>) -> (key: String, value: T) {
        var parent = node.parent
        var output = "\(node.charKey!)"
        while parent != nil {
            if let key = parent?.charKey {
                output = String(key) + output
            }
            
            parent = parent?.parent
        }
        
        return (output, node.value!)
    }
    
    
    public class Node<T> {
        var charKey: Character?
        var value: T?
        
        var parent: Node?
        
        var children: [Character: Node] = [:]
        
        var isEnd: Bool = false
        
        init(charKey: Character?) {
            self.charKey = charKey
            self.value = nil
        }
        
    }
}
