# CSV 기반 타일맵 자동 배치 시스템 (에디터 툴 확장 버전)

## 1. 기능 개요

기존에는 Unity Inspector에서 수동으로 CSV 파일명을 지정하고 버튼을 눌러야 했다면, 이번 개선을 통해 **Unity 상단 메뉴바의 `Tools` 툴바 기능을 활용**하여 지정된 폴더 경로에 있는 모든 CSV 파일을 일괄 스캔하고, 자동으로 타일맵 데이터(`TileMapData` 에셋)를 생성하도록 시스템을 전면 개편하였다.

이를 통해 여러 개의 스테이지 CSV 파일이 추가되더라도 에디터 메뉴에서 클릭 한 번으로 타일맵 데이터 세팅을 자동화할 수 있다.

---

## 2. AI 활용 및 입력 프롬프트

**활용 도구:** ChatGPT

**활용 목적:** Unity 상단 메뉴바에 전용 `Tools` 기능을 추가하고, 특정 폴더 내의 모든 CSV 파일을 자동으로 읽어 타일맵 데이터 에셋을 일괄 생성하는 에디터 확장 스크립트 작성

### 수정 요청 프롬프트

> 내가 원하는 방식은 상단에 파일 에딧 에셋 있는 그칸에 tool이라는걸 만들어서 특정 파일경로에있는 csv를 모두 읽어서 특정 파일위치에 타일맵데이터 를 자동으로 만들어주는걸 만들고 싶어.

### 주요 수정 및 교체 내용

* **기존 스크립트 전면 교체:** 단일 파일 지정 방식의 기존 `TileMapGridLoader` 스크립트를 에디터 확장(`MenuItem`) 기반의 일괄 처리 시스템으로 완전히 교체하였다.
* **`Tools` 메뉴 추가:** Unity 에디터 상단 메뉴에 `Tools/TileMap/Generate TileMap Data from CSV` 항목을 신설하였다.
* **자동 파일 스캔:** 지정된 CSV 폴더(`Assets/Data/CSV`) 안의 모든 파일을 탐색하여 일괄 처리하도록 구현하였다.
* **자동 에셋 생성/갱신:** 읽어온 데이터를 기반으로 프로젝트 경로 내에 `TileMapData` ScriptableObject 에셋을 자동으로 생성하거나 갱신하도록 개선하였다.

---

## 3. 타일 코드 매핑 규칙 및 데이터 처리 방식

CSV 내부에 작성된 숫자와 타일 ID 간의 매핑 규칙은 다음과 같다.

| 숫자 코드 | 타일 ID | 설명 |
| :--- | :--- | :--- |
| **0** | 없음 | 빈 공간으로 처리 |
| **1** | Ground | 지면 타일 |
| **2** | Bridge | 다리 타일 |
| **3** | Platform | 플랫폼 타일 |

---

## 4. 에디터 툴(`Tools`)을 활용한 자동화 구현 내용

### 4-1. `Tools` 메뉴 추가 및 파일 일괄 스캔

Unity 에디터 상단 `Tools` 메뉴에 진입하여 스크립트에 정의된 함수를 실행하면, 지정된 폴더 경로(`Assets/Data/CSV`) 내의 모든 `.csv` 파일을 자동으로 탐색한다.

```csharp
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class TileMapCSVEditorTool
{
    private static readonly string CSVFolderPath = "Assets/Data/CSV";
    private static readonly string OutputFolderPath = "Assets/Data/TileMapData";

    [MenuItem("Tools/TileMap/Generate TileMap Data from CSV")]
    public static void GenerateTileMapDataFromAllCSV()
    {
        if (!Directory.Exists(CSVFolderPath))
        {
            Debug.LogError($"CSV 폴더를 찾을 수 없습니다: {CSVFolderPath}");
            return;
        }

        if (!Directory.Exists(OutputFolderPath))
        {
            Directory.CreateDirectory(OutputFolderPath);
        }

        string[] csvFiles = Directory.GetFiles(CSVFolderPath, "*.csv");

        if (csvFiles.Length == 0)
        {
            Debug.LogWarning("처리할 CSV 파일이 없습니다.");
            return;
        }

        foreach (var filePath in csvFiles)
        {
            ProcessCSVToTileMapData(filePath);
        }

        AssetDatabase.Refresh();
        Debug.Log($"[TileMap Editor Tool] 총 {csvFiles.Length}개의 CSV 파일 타일맵 데이터 변환 완료");
    }
}
```

### 4-2. CSV 파싱 및 `TileMapData` 에셋 자동 생성

각 CSV 파일을 읽어 행과 열을 분석하고, Unity 셀 좌표로 변환하여 `TileMapData` 에셋으로 저장한다.

```csharp
private static void ProcessCSVToTileMapData(string csvFilePath)
{
    string fileName = Path.GetFileNameWithoutExtension(csvFilePath);
    string assetPath = $"{OutputFolderPath}/{fileName}_Data.asset";

    // 기존 데이터 에셋이 존재하면 로드, 없으면 새로 생성
    TileMapData tileMapData = AssetDatabase.LoadAssetAtPath<TileMapData>(assetPath);
    if (tileMapData == null)
    {
        tileMapData = ScriptableObject.CreateInstance<TileMapData>();
        AssetDatabase.CreateAsset(tileMapData, assetPath);
    }

    string[] lines = File.ReadAllLines(csvFilePath);
    tileMapData.tiles.Clear();

    int totalRows = lines.Length;

    // 숫자 코드 매핑 사전 정의
    Dictionary<int, string> mappingCache = new Dictionary<int, string>()
    {
        { 1, "Ground" },
        { 2, "Bridge" },
        { 3, "Platform" }
    };

    for (int y = 0; y < totalRows; y++)
    {
        string line = lines[y];
        if (string.IsNullOrWhiteSpace(line)) continue;

        string[] values = line.Split(',');

        for (int x = 0; x < values.Length; x++)
        {
            if (int.TryParse(values[x].Trim(), out int tileCode))
            {
                if (tileCode == 0) continue; // 빈 공간 생략

                if (mappingCache.TryGetValue(tileCode, out string tileID))
                {
                    // 엑셀 행/열 좌표를 Unity 셀 좌표계로 변환 (Y축 반전)
                    int cellX = x;
                    int cellY = totalRows - 1 - y;

                    tileMapData.tiles.Add(new TileData
                    {
                        cellPosition = new Vector3Int(cellX, cellY, 0),
                        tileID = tileID
                    });
                }
                else
                {
                    Debug.LogWarning($"[TileMap Editor Tool] 매핑되지 않은 코드 발견 ({tileCode}) - 파일: {fileName}");
                }
            }
        }
    }

    EditorUtility.SetDirty(tileMapData);
    Debug.Log($"[TileMap Editor Tool] 생성 완료: {assetPath}");
}
```

---

## 5. 전체 동작 과정 (개선된 방식)

1. 엑셀에서 타일 배치 숫자를 작성하고 `Assets/Data/CSV` 폴더에 CSV 파일로 저장한다.
2. Unity 에디터 상단 메뉴에서 **`Tools` > `TileMap` > `Generate TileMap Data from CSV`** 를 클릭한다.
3. 스크립트가 지정된 폴더 내의 모든 CSV 파일을 자동으로 탐색한다.
4. 각 CSV 파일의 데이터를 분석하여 숫자 코드를 타일 ID로 변환하고 좌표를 계산한다.
5. 계산된 타일 정보를 바탕으로 `Assets/Data/TileMapData` 폴더 내에 `TileMapData` 에셋을 자동으로 생성하거나 갱신한다.
6. 기존 `TileMapSaveLoad` 시스템에 생성된 `TileMapData` 에셋을 연결하여 스테이지를 구성한다.

---

## 6. 구현 구조 및 역할 분리

| 스크립트 / 에셋 | 역할 |
| :--- | :--- |
| `TileMapCSVEditorTool.cs` | 상단 `Tools` 메뉴 연동, CSV 파일 일괄 탐색 및 타일맵 에셋 자동 생성 |
| `TileMapData.cs` | 타일 배치 데이터 보관 (ScriptableObject) |
| `TileMapSaveLoad.cs` | 타일 데이터 로드 및 실제 오브젝트 생성 |
| `TileMapTileDefinition` | 타일 ID와 타일 프리팹 등의 정의 정보 관리 |

---

## 7. 구현 결과 및 기대 효과

상단 메뉴바 기반의 툴(`Tools`) 도입을 통해 기존의 수동 컴포넌트 지정 방식을 탈피하고 대량의 스테이지 데이터를 일괄 관리할 수 있는 구조를 확립하였다.

* **에디터 작업 편의성 향상:** 상단 메뉴 클릭 한 번으로 모든 CSV 파일이 타일맵 데이터 에셋으로 자동 변환된다.
* **대규모 데이터 관리 용이:** 복수의 스테이지 CSV 파일을 일괄 처리할 수 있어 휴먼 에러를 방지한다.
* **기존 시스템과의 유기적 연동:** 자동 생성된 `TileMapData` 에셋은 기존 `TileMapSaveLoad` 및 `TileMapTileDefinition` 구조와 완벽하게 호환된다.

---

## 8. 향후 개선 사항

* CSV 파일의 행과 열이 일정하지 않을 경우에 대한 예외 처리
* 잘못된 숫자 및 데이터 형식에 대한 검증 강화
* 동일한 숫자 코드가 중복 등록되었을 때 경고를 출력하는 기능
* 타일 배치 완료 후 `TileColliderMerger`와 연동하여 충돌 영역 최적화
* 에디터에서 CSV 파일을 선택하고 즉시 배치 결과를 확인할 수 있는 기능
